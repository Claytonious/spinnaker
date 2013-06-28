using System;
using Spinnaker.Core;
using System.Windows.Forms;
using Awesomium.Windows.Forms;

namespace Spinnaker.Bridge.Awesomium.WinForms
{
	public class AwesomiumWinFormsSpinnakerConfiguration : SpinnakerConfiguration
	{
		static AwesomiumWinFormsSpinnakerConfiguration()
		{
			currentConfig = new AwesomiumWinFormsSpinnakerConfiguration();
		}

		public static void Init()
		{
		}

        protected AwesomiumWinFormsSpinnakerConfiguration()
			: base()
		{
            hostScriptObjectName = "spinnakerHost";
			bridgeCreator = (o) => new AwesomiumBrowserBridge((WebControl)o);
			InitCommon();
		}
	}
}

