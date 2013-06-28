using System;
using MonoTouch.UIKit;
using Spinnaker.Core;
using System.Text;
using MonoTouch.Foundation;

namespace Spinnaker.Bridge.MonoTouch
{
	public class MonoTouchBrowserBridge : BrowserBridge
	{
		internal UIWebView webBrowser;

		private Action loadedHandler;
		
		static MonoTouchBrowserBridge()
		{
			defaultViewsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/../SpinnakerSampleiPad.app/Views";
		}
		
		public MonoTouchBrowserBridge (UIWebView webBrowser)
			: base()
		{
			this.webBrowser = webBrowser;
			webBrowser.Delegate = new ScriptObject(this);
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
			InsertScript("function LocationMessenger() {\n" +
			             "    var self = this;\n" +
			             "    self.pendingArg = '';\n" +
			             "    self.sendMessage = function(argsToSend) {\n" +
			             "        self.pendingArg = JSON.stringify(argsToSend);\n" +
			             "        var iframe = document.createElement(\"IFRAME\");\n" +
			             "        iframe.setAttribute(\"src\", \"js-frame\");\n" +
			             "        document.documentElement.appendChild(iframe);\n" +
			             "        iframe.parentNode.removeChild(iframe);\n" +
			             "        iframe = null;\n" +
			             "    };\n" +
			             "    self.HandleScriptPropertyChanged = function(id,propertyName,newValue) {\n" +
			             "        var argsToSend = ['HandleScriptPropertyChanged',id,propertyName,newValue];\n" +
			             "        self.sendMessage(argsToSend);\n" +
			             "    };\n" +
			             "    self.InvokeViewModelIndexMethod = function(id, methodName, p) {\n" +
			             "        var argsToSend = ['InvokeViewModelIndexMethod',id,methodName,p];\n" +
			             "        self.sendMessage(argsToSend);\n" +
			             "    };\n" +
			             "    self.InvokeViewModelMethod = function(id, methodName) {\n" +
			             "        var argsToSend = ['InvokeViewModelMethod',id,methodName];\n" +
			             "        self.sendMessage(argsToSend);\n" +
			             "    };\n" +
			             "    self.HandleDocumentReady = function() {\n" +
			             "        var argsToSend = ['HandleDocumentReady'];\n" +
			             "        self.sendMessage(argsToSend);\n" +
			             "    };\n" +
			             "    self.HostLog = function(msg) {\n" +
			             "        var argsToSend = ['HostLog',msg];\n" +
			             "        self.sendMessage(argsToSend);\n" +
			             "    };\n" +
			             "}\n" +
			             "window.locationMessenger = new LocationMessenger();\n");
			base.EnhanceDOM (viewFilename);
		}
		
		public override void ExecuteScriptFunction (string functionName, string arg)
		{
			ExecuteScriptFunction(functionName, new object[] { arg });
		}
		
		public override void ExecuteScriptFunction (string functionName, params object[] args)
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append (functionName).Append ("(");
			for (int i = 0; i < args.Length; i++) 
			{
				if (args[i] == null)
					builder.Append("null");
				else
					builder.Append("'").Append(args[i].ToString()).Append("'");
				if (i != args.Length - 1)
					builder.Append(",");
			}
			builder.Append(");");
			webBrowser.EvaluateJavascript(builder.ToString());
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
			webBrowser.LoadRequest(new NSUrlRequest(uri));
		}
	}
}

