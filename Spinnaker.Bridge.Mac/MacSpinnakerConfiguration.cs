using System;
using Spinnaker.Core;
using MonoMac.WebKit;

namespace Spinnaker.Bridge.Mac
{
	public class MacSpinnakerConfiguration : SpinnakerConfiguration
	{
		static MacSpinnakerConfiguration()
		{
			currentConfig = new MacSpinnakerConfiguration();
		}

		public static void Init()
		{
		}

		protected MacSpinnakerConfiguration()
			: base()
		{
			hostScriptFunctionSuffix = "_";
			hostScriptObjectName = "window.spinnaker";
			bridgeCreator = (o) => new MonoMacBrowserBridge((WebView)o);
			InitCommon();
		}
	}
}
