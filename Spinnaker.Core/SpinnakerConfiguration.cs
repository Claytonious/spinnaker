using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Spinnaker.Core
{
    public class SpinnakerConfiguration
    {
		protected static SpinnakerConfiguration currentConfig;

		private ViewModelManager viewModelManager;
		protected Func<object,BrowserBridge> bridgeCreator;
		protected string hostScriptObjectName;
		protected string hostScriptFunctionSuffix;

		protected SpinnakerConfiguration()
		{
		}

		public static SpinnakerConfiguration CurrentConfig
		{
			get { return currentConfig; }
		}

        public Action<INotifyPropertyChanged, Action> PropertyChangeHandler { get; set; }

		protected void InitCommon()
		{
			LogLevel = SpinnakerLogLevel.Warn;
			LogHandler = (logLevel, msg, ex) =>
			{
				if (logLevel >= LogLevel)
					Console.Out.WriteLine("Spinnaker [" + logLevel + "]: " + msg);
			};
		}

		public BrowserBridge CreateBrowserBridge (object host)
		{
			return bridgeCreator(host);
		}

		public ViewModelManager ViewModelManager 
		{ 
			get 
			{
				if (viewModelManager == null)
					viewModelManager = new ViewModelManager();
				return viewModelManager; 
			} 
		}

		public SpinnakerLogLevel LogLevel { get; set; }
        public Action<SpinnakerLogLevel, string, Exception> LogHandler { get; set; }        

		public void Log(SpinnakerLogLevel logLevel, string msg)
        {
            LogHandler(logLevel, msg, null);
        }

        public void Log(SpinnakerLogLevel logLevel, string msg, Exception ex)
        {
            LogHandler(logLevel, msg, ex);
        }

		public string HostScriptObjectName 
		{
			get { return hostScriptObjectName; }
		}

		public string HostScriptFunctionSuffix 
		{
			get { return hostScriptFunctionSuffix; }
		}
    }
}
