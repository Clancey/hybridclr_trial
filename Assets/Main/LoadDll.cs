using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDll : MonoBehaviour
{


    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
    };

    void Start()
    {
        StartCoroutine(DownLoadAssets(this.StartGame));
    }

  private static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();
  private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

  static string GetWebRequestPath(string asset) {
        var path = $"{Application.streamingAssetsPath}/{asset}";
        if (!path.Contains("://"))
        {
            path = "file://" + path;
        }
        if (path.EndsWith(".dll"))
        {
            path += ".bytes";
        }
        return path;
    }

  static async Task<byte[]> GetStreamingAsset(string bundleName, string name) {
    var bundle = await GetStreamingAssetBundle(bundleName);
    var fileData = bundle.LoadAsset<TextAsset>(name);
    return fileData.bytes;

  }
  public static async Task<AssetBundle> GetStreamingAssetBundle(string bundleName) {
    if (assetBundles.ContainsKey(bundleName))
      return assetBundles[bundleName];
    var data = await GetStreamingAssetData(bundleName);
    return assetBundles[bundleName] = AssetBundle.LoadFromMemory(data);
  }
    static async Task<byte[]> GetStreamingAssetData(string bundleName) {
    if (s_assetDatas.ContainsKey(bundleName))
      return s_assetDatas[bundleName];
    //Lets ditch this so it doesn't use unity's http stack...
    string dllPath = GetWebRequestPath(bundleName);
    Debug.Log($"start download ab:{bundleName} 1");
    UnityWebRequest www = UnityWebRequest.Get(dllPath);

    var tcs = new TaskCompletionSource<bool>();
    var request = www.SendWebRequest();
    void onComplete(AsyncOperation operation) => tcs.TrySetResult(true);
    request.completed += onComplete;
    await tcs.Task;
    request.completed -= onComplete;
#if UNITY_2020_1_OR_NEWER
    if (www.result != UnityWebRequest.Result.Success) {
      Debug.Log(www.error);
      return null;
    }
#else
    if (www.isHttpError || www.isNetworkError)
    {
      Debug.Log(www.error);
      return null;
    }
#endif
    else {
      // Or retrieve results as binary data
      byte[] abBytes = www.downloadHandler.data;
      Debug.Log($"dll:{bundleName}  size:{abBytes.Length}");
      return s_assetDatas[bundleName] = abBytes;
    }
  }

  static async Task<Assembly> GetAssembly(string name) {

    var data = await GetStreamingAssetData(name);
#if !UNITY_EDITOR
      return System.Reflection.Assembly.Load(data);
#else
    var n = Path.GetFileNameWithoutExtension(name);
    return AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == n);
#endif
  }


  IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = new List<string>
        {
           // "prefabs",
            "Assembly-CSharp.dll",
        }.Concat(AOTMetaAssemblyNames);

        foreach (var asset in assets)
        {
            string dllPath = GetWebRequestPath(asset);
            Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest www = UnityWebRequest.Get(dllPath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
#else
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
#endif
            else
            {
                // Or retrieve results as binary data
                byte[] assetData = www.downloadHandler.data;
                Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_assetDatas[asset] = assetData;
               // assetBundles[asset] = AssetBundle.LoadFromMemory(assetData);
            }
        }

        onDownloadComplete();
    }


    async void StartGame()
    {
        Debug.Log("Start Game Called");
        await LoadMetadataForAOTAssemblies();
        Debug.Log("Loading ASsembly: Assembly-CSharp");
        var gameAss = await GetAssembly("Assembly-CSharp.dll");

        var appType = gameAss.GetType("AppLoader");
        Debug.Log($"AppLoader Loaded: {appType != null}");
        var mainMethod = appType.GetMethod("Init");
        Debug.Log($"Init Method Loaded: {mainMethod != null}");
        mainMethod.Invoke(null, null);
  }


    /// <summary>
    /// Îªaot assembly¼ÓÔØÔ­Ê¼metadata£¬ Õâ¸ö´úÂë·Åaot»òÕßÈÈ¸üÐÂ¶¼ÐÐ¡£
    /// Ò»µ©¼ÓÔØºó£¬Èç¹ûAOT·ºÐÍº¯Êý¶ÔÓ¦nativeÊµÏÖ²»´æÔÚ£¬Ôò×Ô¶¯Ìæ»»Îª½âÊÍÄ£Ê½Ö´ÐÐ
    /// </summary>
    private static async Task LoadMetadataForAOTAssemblies()
    {
        // ¿ÉÒÔ¼ÓÔØÈÎÒâaot assemblyµÄ¶ÔÓ¦µÄdll¡£µ«ÒªÇódll±ØÐëÓëunity build¹ý³ÌÖÐÉú³ÉµÄ²Ã¼ôºóµÄdllÒ»ÖÂ£¬¶ø²»ÄÜÖ±½ÓÊ¹ÓÃÔ­Ê¼dll¡£
        // ÎÒÃÇÔÚBuildProcessorsÀïÌí¼ÓÁË´¦Àí´úÂë£¬ÕâÐ©²Ã¼ôºóµÄdllÔÚ´ò°üÊ±×Ô¶¯±»¸´ÖÆµ½ {ÏîÄ¿Ä¿Â¼}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} Ä¿Â¼¡£

        /// ×¢Òâ£¬²¹³äÔªÊý¾ÝÊÇ¸øAOT dll²¹³äÔªÊý¾Ý£¬¶ø²»ÊÇ¸øÈÈ¸üÐÂdll²¹³äÔªÊý¾Ý¡£
        /// ÈÈ¸üÐÂdll²»È±ÔªÊý¾Ý£¬²»ÐèÒª²¹³ä£¬Èç¹ûµ÷ÓÃLoadMetadataForAOTAssembly»á·µ»Ø´íÎó
        /// 
        foreach (var aotDllName in AOTMetaAssemblyNames)
        {
      try {
                HomologousImageMode mode = HomologousImageMode.SuperSet;
                byte[] dllBytes = await GetStreamingAssetData(aotDllName);
        // ¼ÓÔØassembly¶ÔÓ¦µÄdll£¬»á×Ô¶¯ÎªËühook¡£Ò»µ©aot·ºÐÍº¯ÊýµÄnativeº¯Êý²»´æÔÚ£¬ÓÃ½âÊÍÆ÷°æ±¾´úÂë
        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
        Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
      }
      catch {
        //Debug.Log(e);
      }
        }
    }
}
