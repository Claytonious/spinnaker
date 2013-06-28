using System;
using MonoMac.Foundation;

namespace Spinnaker.Sample.Mac
{
	public class ScriptObject : NSObject
	{
		public ScriptObject()
		{
		}

		[Export("HostLog:")]
		public void HostLog(string msg)
		{
			Console.Out.WriteLine("BROWSER: " + msg);
			webBrowser.WindowScriptObject.EvaluateWebScript("sayHi('dodo');");
		}
		
		[Export ("isSelectorExcludedFromWebScript:")]
		public static bool IsSelectorExcludedFromWebScript(MonoMac.ObjCRuntime.Selector aSelector)
		{
			return false;
		}
	}
}

