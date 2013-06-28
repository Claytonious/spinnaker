using System;
using WebKit;
using Spinnaker.Core;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Spinnaker.Bridge.Linux
{
	public class LinuxBrowserBridge : BrowserBridge
	{
		private WebView webBrowser;
		private Action loadedHandler;

		static LinuxBrowserBridge()
		{
			defaultViewsPath = Path.Combine(new FileInfo(typeof(LinuxBrowserBridge).Assembly.Location).DirectoryName, "Views");
		}

		public LinuxBrowserBridge (WebView webBrowser)
			: base()
		{
			this.webBrowser = webBrowser;
			webBrowser.LoadFinished += delegate
			{
				webBrowser.TitleChanged += delegate(object o, TitleChangedArgs titleChangeArgs) 
				{
					string pendingArgs = titleChangeArgs.Title;
					JArray argsJArray = (JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(pendingArgs);
					string[] args = new string[argsJArray.Count];
					for (int i = 0; i < argsJArray.Count; i++)
						args[i] = (string)argsJArray[i];
					if (args[0] == "HandleScriptPropertyChanged")
					{
						if (args.Length == 3)
							HandleScriptPropertyChanged(args[1], args[2], null);
						else
							HandleScriptPropertyChanged(args[1], args[2], args[3]);
					}
					else if (args[0] == "InvokeViewModelIndexMethod")
						InvokeViewModelMethod(args[1], args[2], args[3]);
					else if (args[0] == "InvokeViewModelMethod")
						InvokeViewModelMethod(args[1], args[2]);
					else if (args[0] == "HandleDocumentReady")
						HandleDocumentReady();
					else if (args[0] == "HostLog")
						HostLog(args[1]);
					else
						SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Error,"Ignoring unrecognized call from javascript for [" + args[0] + "] - this means that javascript in the view is trying to reach the host but with bad arguments");
				};
				HandleDocumentReady();
			};
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
			             "        document.title = self.pendingArg;\n" +
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
			StringBuilder b = new StringBuilder();
			b.Append("(");
			for (int i = 0; i < args.Length; i++)
			{
				b.Append(StringifyArg(args[i]));
				if (i < args.Length - 1)
					b.Append(",");
			}
			b.Append(")");
			webBrowser.ExecuteScript(functionName + b.ToString());
		}
		
		private string StringifyArg(object arg)
		{
			string result = null;
			if (arg == null)
				result = "null";
			else
				result = "'" + arg.ToString() + "'";
			return result;
		}

		public override void InvokeOnBrowserSafeThread (Action a)
		{
			a();
		}

		protected override void LoadUrl(Uri uri, Action onLoaded)
		{
			this.loadedHandler = onLoaded;
			webBrowser.Open(uri.ToString());
		}
	}
}

