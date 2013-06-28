using System;
using MonoMac.WebKit;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Spinnaker.Core;
using System.Text;

namespace Spinnaker.Bridge.Mac
{
	public class MonoMacBrowserBridge : BrowserBridge
	{
		private WebView webBrowser;
		private ScriptObject scriptObject;
		private Action loadedHandler;

		static MonoMacBrowserBridge()
		{
			defaultViewsPath = NSBundle.MainBundle.BundlePath + "/Contents/Resources/Views";
		}

		public MonoMacBrowserBridge (WebView webBrowser)
			: base()
		{
			this.webBrowser = webBrowser;
			scriptObject = new ScriptObject(this);
			string hostObjectName = SpinnakerConfiguration.CurrentConfig.HostScriptObjectName.Replace("window.",String.Empty);
			webBrowser.WindowScriptObject.SetValueForKey(scriptObject, new NSString(hostObjectName));
			webBrowser.UIGetContextMenuItems = null;
		}

		internal void HandleDocumentReady()
		{
			if (loadedHandler != null)
			{
				loadedHandler();
				loadedHandler = null;
			}
		}

		protected override void EnhanceDOM (string viewFilename)
		{
			base.EnhanceDOM (viewFilename);
			// Can't rely on WebView's FinishLoading event with a local file, so we
			// use jquery in the page, instead...
			InsertScript("$(function () {\n" +
				SpinnakerConfiguration.CurrentConfig.HostScriptObjectName +
					".HandleDocumentReady" + SpinnakerConfiguration.CurrentConfig.HostScriptFunctionSuffix + "();\n" +
			    "});\n");
		}

		public override void ExecuteScriptFunction (string functionName, string arg)
		{
			ExecuteScriptFunction(functionName, new object[] { arg });
		}

		public override void ExecuteScriptFunction (string functionName, params object[] args)
		{
			if (args == null)
				args = new object[0];
			NSObject[] nsArgs = new NSObject[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] != null)
				{
					if (args[i] is Int32)
						nsArgs[i] = new NSNumber((int)args[i]);
					else if (args[i] is Boolean)
						nsArgs[i] = new NSNumber(((bool)args[i]) ? 1 : 0);
					nsArgs[i] = new NSString(args[i].ToString());
				}
				else
					nsArgs[i] = new NSNull();
			}
			webBrowser.WindowScriptObject.CallWebScriptMethod(functionName, nsArgs);
		}

		public override void InvokeOnBrowserSafeThread (Action a)
		{
			if (!NSThread.IsMain)
				webBrowser.InvokeOnMainThread(delegate() { a(); });
			else
				a();
		}

		protected override void LoadUrl(Uri uri, Action onLoaded)
		{
			this.loadedHandler = onLoaded;
			webBrowser.MainFrameUrl = uri.ToString();
		}
	}
}

