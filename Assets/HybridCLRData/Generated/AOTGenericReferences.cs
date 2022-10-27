public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ constraint implement type
	// }} 

	// {{ AOT generic type
	//System.Collections.Generic.List`1<System.Object>
	//System.Runtime.CompilerServices.TaskAwaiter`1<System.Object>
	//System.Threading.Tasks.Task`1<System.Object>
	// }}

	public void RefMethods()
	{
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder::AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter`1<System.Object>,AppLoader/<LoadGame>d__1>(System.Runtime.CompilerServices.TaskAwaiter`1<System.Object>&,AppLoader/<LoadGame>d__1&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder::Start<AppLoader/<LoadGame>d__1>(AppLoader/<LoadGame>d__1&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder::AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AppLoader/<Init>d__0>(System.Runtime.CompilerServices.TaskAwaiter&,AppLoader/<Init>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder::Start<AppLoader/<Init>d__0>(AppLoader/<Init>d__0&)
		// System.Object UnityEngine.GameObject::AddComponent<System.Object>()
	}
}