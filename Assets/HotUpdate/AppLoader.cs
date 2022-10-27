using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Reflection;

public class AppLoaderData {
  public class AssemblyInfo {
    public string AssetBundleName { get; set; }
    public string Name { get; set; }

    public List<string> InitClasses { get; set; } = new List<string>();
  }
  public List<AssemblyInfo> Assemblies { get; set; } = new List<AssemblyInfo>();
  public string InitialScene { get; set; }
}
public static class AppLoader {
  public static async void Init() {
    await LoadGame();
  }

  static async Task LoadGame() {
    Debug.Log("Load Game In Assembly Called!");
    //Load the Scenes 
    var sceneAb = await LoadDll.GetStreamingAssetBundle("prefabs");
    Debug.Log("prefabs loaded");

    string[] scenePaths = sceneAb.GetAllScenePaths();
    Debug.Log("Get all scenes");
    foreach (var s in scenePaths)
      Debug.Log($"Scene: {s}");
    SceneManager.LoadScene("_Client");

  }

}
