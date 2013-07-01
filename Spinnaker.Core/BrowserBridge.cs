using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Spinnaker.Core
{
    public abstract class BrowserBridge
    {
        protected static string defaultViewsPath;

        private static string renderPath;
        private static string renderHtmlFilename;
        private string viewsPath;
        private INotifyPropertyChanged rootViewModel;
        private static readonly string pathSepString = String.Empty + Path.DirectorySeparatorChar;
        private ViewModelManager viewModelManager = SpinnakerConfiguration.CurrentConfig.ViewModelManager;

        static BrowserBridge()
        {
            renderPath = System.IO.Path.GetTempPath();
            if (!renderPath.EndsWith(pathSepString))
                renderPath += pathSepString;
            renderHtmlFilename = renderPath + "Content.html";
        }

        public void HostLog(string msg)
        {
			SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Debug, msg);
        }

        protected BrowserBridge()
        {
            ViewsPath = defaultViewsPath;
        }

        public void InsertScript(string script)
        {
            string content = File.ReadAllText(renderHtmlFilename);
            content = content.Replace("</body>", "<script type=\"text/javascript\">\n" + script + "\n</script>\n</body>");
			// We have to delete the file before replacing it on some platforms (e.g. Mac would have intermittent
			// sharing violation on the file without this)
			File.Delete(renderHtmlFilename);
            File.WriteAllText(renderHtmlFilename, content);
        }

        public abstract void ExecuteScriptFunction(string functionName, string arg);
        public abstract void ExecuteScriptFunction(string functionName, params object[] args);
        public abstract void InvokeOnBrowserSafeThread(Action a);
        protected abstract void LoadUrl(Uri uri, Action onLoaded);

        public void HandleScriptPropertyChanged(string id, string propertyName, string newValue)
        {
            viewModelManager.HandleScriptPropertyChanged(id, propertyName, newValue);
        }

        public void InvokeViewModelMethod(string id, string methodName)
        {
            viewModelManager.InvokeViewModelMethod(id, methodName);
        }

        public void InvokeViewModelMethod(string id, string methodName, string arg)
        {
            viewModelManager.InvokeViewModelMethod(id, methodName, arg);
        }

        public string ViewsPath
        {
            get { return viewsPath; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentException("Cannot be null or empty", "ViewsPath");
                if (!Directory.Exists(value))
                    throw new ArgumentException("Must be a valid directory that is visible to this process", "ViewsPath");
                viewsPath = value;
                if (!viewsPath.EndsWith(pathSepString))
                    viewsPath += pathSepString;
            }
        }

        protected virtual void EnhanceDOM(string viewFilename)
        {
			//viewModelManager.Reset();
            viewModelManager.BindViewModel(rootViewModel, this);
        }

        public void ShowView(string viewName, INotifyPropertyChanged viewModel)
        {
            ShowView(viewName, viewModel, null);
        }

        public void ShowView(string viewName, INotifyPropertyChanged viewModel, Action onShown)
        {
            if (String.IsNullOrEmpty(viewName))
                throw new ArgumentException("Cannot be null or empty", "viewName");
            string path = viewsPath + viewName;
            if (!File.Exists(path))
                throw new ArgumentException("Can't find HTML file for view", "viewName");

            CopyDirectory(defaultViewsPath, renderPath);
            
            this.rootViewModel = viewModel;
            renderHtmlFilename = Path.Combine(renderPath, "Content.html");
            File.Copy(path, renderHtmlFilename, true);
            EnhanceDOM(renderHtmlFilename);
            SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Debug, "Rendering content to [" + renderHtmlFilename + "]");
            LoadUrl(new Uri("file://" + renderHtmlFilename), () =>
            {
                InvokeOnBrowserSafeThread(() =>
                {
                    viewModelManager.ActivateRootViewModel(viewModel, this);
                    if (onShown != null)
                        onShown();
                    ExecuteScriptFunction("handleSpinnakerReady", "");
                });
            });
        }

        private static void CopyDirectory (string sourceDirName, string destDirName)
		{
			DirectoryInfo dir = new DirectoryInfo (sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories ();

			if (!dir.Exists)
				throw new DirectoryNotFoundException ("Source directory does not exist or could not be found: " + sourceDirName);

			if (!Directory.Exists (destDirName))
				Directory.CreateDirectory (destDirName);

			FileInfo[] files = dir.GetFiles ();
			foreach (FileInfo file in files) 
				file.CopyTo (Path.Combine (destDirName, file.Name), true);

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }
    }
}
