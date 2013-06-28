using Microsoft.Win32;
using Spinnaker.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spinnaker.Bridge.WinForms
{
    public class WinFormsBrowserBridge : BrowserBridge
    {
        private WebBrowser browser;
        private Action loadedHandler;
        private ScriptObject scriptObject;

        static WinFormsBrowserBridge()
        {
            defaultViewsPath = Path.Combine(new FileInfo(typeof(WinFormsBrowserBridge).Assembly.Location).DirectoryName, "Views");
        }

        public WinFormsBrowserBridge (WebBrowser browser)
            : base()
		{
            this.browser = browser;
            browser.Navigated += HandleNavigated;
            browser.DocumentCompleted += HandleDocumentCompleted;
            scriptObject = new ScriptObject(this);
            browser.ObjectForScripting = scriptObject;
        }

        protected override void EnhanceDOM(string viewFilename)
        {
            try
            {
                if (browser.Version.Major == 10)
                {
                    string html = File.ReadAllText(viewFilename);
                    html = html.Replace("<head>", "<head>\n\t<meta http-equiv=\"X-UA-Compatible\" content=\"IE=10\" />");
                    File.WriteAllText(viewFilename, html);
                }
                else if (browser.Version.Major == 9)
                {
                    string html = File.ReadAllText(viewFilename);
                    html = html.Replace("<head>", "<head>\n\t<meta http-equiv=\"X-UA-Compatible\" content=\"IE=9\" />");
                    File.WriteAllText(viewFilename, html);
                }
                else
                {
                    string html = File.ReadAllText(viewFilename);
                    html = html.Replace("<head>", "<head>\n\t<meta http-equiv=\"X-UA-Compatible\" content=\"IE=8\" />");
                    File.WriteAllText(viewFilename, html);
                }
            }
            catch (Exception)
            {
            }
           
            base.EnhanceDOM(viewFilename);
        }

        private void HandleDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (loadedHandler != null)
            {
                loadedHandler();
                loadedHandler = null;
            }
        }

        public override void InvokeOnBrowserSafeThread(Action a)
        {
            if (browser.InvokeRequired)
                browser.Invoke(new MethodInvoker(() => InvokeOnBrowserSafeThread(a)));
            else
                a();
        }

        private void HandleNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
        }

        protected override void LoadUrl(Uri uri, Action onLoaded)
        {
            loadedHandler = onLoaded;
            browser.Url = uri;
        }

        public override void ExecuteScriptFunction(string functionName, params object[] args)
        {
            InvokeOnBrowserSafeThread(() =>
            {
                browser.Document.InvokeScript(functionName, args);
            });
        }

        public override void ExecuteScriptFunction(string functionName, string arg)
        {
            InvokeOnBrowserSafeThread(() =>
            {
                browser.Document.InvokeScript(functionName, new object[] { arg });
            });
        }
    }
}
