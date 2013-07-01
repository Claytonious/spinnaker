using Awesomium.Core;
using Awesomium.Windows.Forms;
using Spinnaker.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Spinnaker.Bridge.Awesomium.WinForms
{
    public class AwesomiumBrowserBridge : BrowserBridge
    {
        private WebControl webControl;
        private Action loadedHandler;
        
        static AwesomiumBrowserBridge()
        {
            defaultViewsPath = Path.Combine(new FileInfo(typeof(AwesomiumBrowserBridge).Assembly.Location).DirectoryName, "Views");
        }

        public AwesomiumBrowserBridge(WebControl webControl)
            : base()
		{
            this.webControl = webControl;
            webControl.DocumentReady += HandleDocumentCompleted;
        }

        private void HandleDocumentCompleted(object sender, UrlEventArgs e)
        {
            JSObject jsobject = webControl.CreateGlobalJavascriptObject("spinnakerHost");
            jsobject.Bind("HandleScriptPropertyChanged", false, HandleJS);
            jsobject.Bind("InvokeViewModelMethod", false, HandleJS);
            jsobject.Bind("InvokeViewModelIndexMethod", false, HandleJS);
            jsobject.Bind("HostLog", false, HandleJS);

            if (loadedHandler != null)
            {
                loadedHandler();
                loadedHandler = null;
            }
        }

        private void HandleJS(object sender, JavascriptMethodEventArgs args)
        {
            if (args.MethodName == "HandleScriptPropertyChanged")
                HandleScriptPropertyChanged(args.Arguments[0].ToString(), args.Arguments[1].ToString(), args.Arguments[2].ToString());
            else if (args.MethodName == "InvokeViewModelMethod")
                InvokeViewModelMethod(args.Arguments[0].ToString(), args.Arguments[1].ToString());
            else if (args.MethodName == "InvokeViewModelIndexMethod")
                InvokeViewModelMethod(args.Arguments[0].ToString(), args.Arguments[1].ToString(), args.Arguments[2].ToString());
            else if (args.MethodName == "HostLog")
                HostLog(args.Arguments[0].ToString());
            else
                SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Error, "Unknown method invoked on host by javascript [" + args.MethodName + "]");
        }

        public override void InvokeOnBrowserSafeThread(Action a)
        {
            if (webControl.InvokeRequired)
                webControl.Invoke(new MethodInvoker(() => InvokeOnBrowserSafeThread(a)));
            else
                a();
        }

        protected override void LoadUrl(Uri uri, Action onLoaded)
        {
            loadedHandler = onLoaded;
            webControl.Source = uri;
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

        public override void ExecuteScriptFunction(string functionName, params object[] args)
        {
            InvokeOnBrowserSafeThread(() =>
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
                webControl.ExecuteJavascript(functionName + b.ToString());
            });
        }

        public override void ExecuteScriptFunction(string functionName, string arg)
        {
            InvokeOnBrowserSafeThread(() =>
            {
                webControl.ExecuteJavascript(functionName + "('" + arg + "')");
            });
        }
    }
}
