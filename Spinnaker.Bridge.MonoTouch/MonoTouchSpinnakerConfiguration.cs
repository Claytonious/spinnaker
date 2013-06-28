using System;
using Spinnaker.Core;
using MonoTouch.UIKit;

namespace Spinnaker.Bridge.MonoTouch
{
	public class MonoTouchSpinnakerConfiguration : SpinnakerConfiguration
	{
		static MonoTouchSpinnakerConfiguration()
		{
			currentConfig = new MonoTouchSpinnakerConfiguration();
		}
		
		public static void Init()
		{
		}
		
		protected MonoTouchSpinnakerConfiguration()
			: base()
		{
			hostScriptObjectName = "locationMessenger";
			bridgeCreator = (o) => new MonoTouchBrowserBridge((UIWebView)o);
			InitCommon();
		}
	}
}

