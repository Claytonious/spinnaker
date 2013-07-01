using Spinnaker.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Spinnaker.Bridge.WPF
{
    public class WPFBrowserBridge : BrowserBridge
    {
        private WebBrowser browser;
        private Action loadedHandler;
        private ScriptObject scriptObject;
        private const int NameNotFoundErrorCode = -2147352570;

        static WPFBrowserBridge()
        {
            defaultViewsPath = Path.Combine(new FileInfo(typeof(WPFBrowserBridge).Assembly.Location).DirectoryName, "Views");
        }

        public WPFBrowserBridge(WebBrowser browser)
            : base()
		{
            this.browser = browser;
            browser.LoadCompleted += HandleDocumentCompleted;
            scriptObject = new ScriptObject(this);
            browser.ObjectForScripting = scriptObject;
        }

        protected override void EnhanceDOM(string viewFilename)
        {
            try
            {
                string html = File.ReadAllText(viewFilename);
                html = html.Replace("<head>", "<head>\n\t<meta http-equiv=\"X-UA-Compatible\" content=\"IE=10\" />");
                File.WriteAllText(viewFilename, html);
            }
            catch { }
            base.EnhanceDOM(viewFilename);
        }

        void HandleDocumentCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (loadedHandler != null)
            {
                loadedHandler();
                loadedHandler = null;
            }
        }

        public override void InvokeOnBrowserSafeThread(Action a)
        {
            if (browser.Dispatcher.CheckAccess())
                a();
            else
                browser.Dispatcher.Invoke(a);
        }

        protected override void LoadUrl(Uri uri, Action onLoaded)
        {
            loadedHandler = onLoaded;
            browser.Navigate(uri);
        }

        public override void ExecuteScriptFunction(string functionName, params object[] args)
        {
            InvokeOnBrowserSafeThread(() =>
            {
                try
                {
                    browser.InvokeScript(functionName, args);
                }
                catch (System.Runtime.InteropServices.COMException comEx)
                {
                    // Name not found is a normal case of a function not existing on the javascript side. Many functions that we call are optional,
                    // so we don't treat this as an error worthy of reporting. Anything else is a surprise, though.
                    if (comEx.ErrorCode != NameNotFoundErrorCode)
                        Spinnaker.Core.SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Warn, "Error invoking javascript " + comEx);
                }
                catch (Exception ex)
                {
                    Spinnaker.Core.SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Warn, "Error invoking javascript " + ex);
                }
            });
        }

        public override void ExecuteScriptFunction(string functionName, string arg)
        {
            ExecuteScriptFunction(functionName, new object[] { arg });
        }
    }
}
