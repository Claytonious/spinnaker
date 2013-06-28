using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using log4net;
using Spinnaker.Core;
using Spinnaker.Log4net;
using Spinnaker.Bridge.Mac;

namespace Spinnaker.Sample.Mac
{
	class MainClass
	{
		static void Main (string[] args)
		{
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}	

