using System;
using Spinnaker.Core;
using System.Windows.Forms;

namespace Spinnaker.Bridge.WinForms
{
	public class WinFormsSpinnakerConfiguration : SpinnakerConfiguration
	{
		static WinFormsSpinnakerConfiguration()
		{
			currentConfig = new WinFormsSpinnakerConfiguration();
		}

		public static void Init()
		{
		}

        protected WinFormsSpinnakerConfiguration()
			: base()
		{
            hostScriptObjectName = "window.external";
			bridgeCreator = (o) => new WinFormsBrowserBridge((WebBrowser)o);
			InitCommon();
		}
	}
}

