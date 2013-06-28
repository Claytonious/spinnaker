using System;
using Spinnaker.Core;

namespace Spinnaker.Bridge.Tests
{
	public class TestSpinnakerConfiguration : SpinnakerConfiguration
	{
		static TestSpinnakerConfiguration()
		{
			currentConfig = new TestSpinnakerConfiguration();
		}

		public static void Init()
		{
		}

		protected TestSpinnakerConfiguration()
			: base()
		{
			hostScriptFunctionSuffix = "_";
			hostScriptObjectName = "window.spinnaker";
			bridgeCreator = (o) => new UnitTestBrowserBridge((UnitTestWebBrowser)o);
			InitCommon();
		}
	}
}
