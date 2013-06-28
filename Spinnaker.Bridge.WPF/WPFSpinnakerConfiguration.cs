using System;
using Spinnaker.Core;
using System.Windows.Controls;

namespace Spinnaker.Bridge.WPF
{
	public class WPFSpinnakerConfiguration : SpinnakerConfiguration
	{
		static WPFSpinnakerConfiguration()
		{
			currentConfig = new WPFSpinnakerConfiguration();
		}

		public static void Init()
		{
		}

        protected WPFSpinnakerConfiguration()
			: base()
		{
            hostScriptObjectName = "window.external";
			bridgeCreator = (o) => new WPFBrowserBridge((WebBrowser)o);
			InitCommon();
		}
    }
}

