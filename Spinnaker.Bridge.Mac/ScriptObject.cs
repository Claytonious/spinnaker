using System;
using MonoMac.Foundation;

namespace Spinnaker.Bridge.Mac
{
	public class ScriptObject : NSObject
	{
		private MonoMacBrowserBridge bridge;

		public ScriptObject(MonoMacBrowserBridge bridge)
		{
			this.bridge = bridge;
		}

		[Export("HandleScriptPropertyChanged:")]
		public void HandleScriptPropertyChanged(string id, string propertyName, string newValue)
		{
			bridge.HandleScriptPropertyChanged(id, propertyName, newValue);
		}

		[Export("InvokeViewModelIndexMethod:")]
		public void InvokeViewModelIndexMethod(string id, string methodName, NSObject arg)
		{
			bridge.InvokeViewModelMethod(id, methodName, arg == null ? null :  arg.ToString());
		}

		[Export("InvokeViewModelMethod:")]
		public void InvokeViewModelMethod(string id, string methodName)
		{
			bridge.InvokeViewModelMethod(id, methodName);
		}

		[Export("HostLog:")]
		public void HostLog(string msg)
		{
			bridge.HostLog("- JS - " + msg);
		}

		[Export("HandleDocumentReady:")]
		public void HandleDocumentReady()
		{
			bridge.HandleDocumentReady();
		}

		[Export ("isSelectorExcludedFromWebScript:")]
		public static bool IsSelectorExcludedFromWebScript(MonoMac.ObjCRuntime.Selector aSelector)
		{
			return false;
		}
	}
}

