using System;
using System.IO;
using Spinnaker.Core;
using System.Collections.Generic;

namespace Spinnaker.Bridge.Tests
{
	/// <summary>
	/// This is the browser bridge for the "unit testing" platform - not really a target platform (like Windows, Mac, etc.)
	/// but for use in unit tests.
	/// </summary>
	public class UnitTestBrowserBridge : BrowserBridge
	{
		static UnitTestBrowserBridge()
		{
			defaultViewsPath = Path.Combine(new FileInfo(typeof(UnitTestBrowserBridge).Assembly.Location).DirectoryName, "Views");
		}

		private UnitTestWebBrowser webBrowser;
		private Action loadedHandler;

		public UnitTestBrowserBridge (UnitTestWebBrowser webBrowser)
			: base()
		{
			this.webBrowser = webBrowser;
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
			webBrowser.ExecuteScriptFunction(functionName, args);
		}
		
		public override void InvokeOnBrowserSafeThread (Action a)
		{
			a();
		}
		
		protected override void LoadUrl(Uri uri, Action onLoaded)
		{
			this.loadedHandler = onLoaded;
			webBrowser.Url = uri.ToString();
			HandleDocumentReady();
		}
	}
}

