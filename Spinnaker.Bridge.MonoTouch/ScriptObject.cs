using System;
using MonoTouch.UIKit;
using Spinnaker.Core;
using MonoTouch.Foundation;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Spinnaker.Bridge.MonoTouch
{
	public class ScriptObject : UIWebViewDelegate
	{
		private MonoTouchBrowserBridge bridge;

		public ScriptObject(MonoTouchBrowserBridge bridge)
		{
			this.bridge = bridge;
		}

		public override void LoadFailed (UIWebView webView, NSError error)
		{
		}
		
		public override void LoadingFinished (UIWebView webView)
		{
			bridge.HandleDocumentReady();
		}
		
		public override void LoadStarted (UIWebView webView)
		{
		}
		
		public override bool ShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			bool shouldStart = true;
			string url = request.Url.ToString();
			if (url.Contains("js-frame"))
			{
				shouldStart = false;
				string pendingArgs = bridge.webBrowser.EvaluateJavascript("window.locationMessenger.pendingArg.toString();");
				// Using JsonConvert.Deserialze<string[]> blows up under MonoTouch, so manually build the array of args instead
				JArray argsJArray = (JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(pendingArgs);
				string[] args = new string[argsJArray.Count];
				for (int i = 0; i < argsJArray.Count; i++)
					args[i] = (string)argsJArray[i];
				if (args[0] == "HandleScriptPropertyChanged")
				{
					if (args.Length == 3)
						bridge.HandleScriptPropertyChanged(args[1], args[2], null);
					else
						bridge.HandleScriptPropertyChanged(args[1], args[2], args[3]);
				}
				else if (args[0] == "InvokeViewModelIndexMethod")
					bridge.InvokeViewModelMethod(args[1], args[2], args[3]);
				else if (args[0] == "InvokeViewModelMethod")
					bridge.InvokeViewModelMethod(args[1], args[2]);
				else if (args[0] == "HandleDocumentReady")
					bridge.HandleDocumentReady();
				else if (args[0] == "HostLog")
					bridge.HostLog(args[1]);
				else
					SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Error,"Ignoring unrecognized call from javascript for [" + args[0] + "] - this means that javascript in the view is trying to reach the host but with bad arguments");
			}
			return shouldStart;
		}	
	}
}

