using System;
using Spinnaker.Core;
using WebKit;

namespace Spinnaker.Bridge.Linux
{
	public class LinuxSpinnakerConfiguration : SpinnakerConfiguration
	{
		static LinuxSpinnakerConfiguration()
		{
            currentConfig = new LinuxSpinnakerConfiguration();
		}

		public static void Init()
		{
		}

		protected LinuxSpinnakerConfiguration()
			: base()
		{
			hostScriptObjectName = "locationMessenger";
			bridgeCreator = (o) => new LinuxBrowserBridge((WebView)o);
			InitCommon();
		}
	}
}
