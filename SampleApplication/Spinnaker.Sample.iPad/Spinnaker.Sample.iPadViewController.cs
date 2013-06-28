using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Spinnaker.Bridge.MonoTouch;
using Spinnaker.Sample.WinForms.ViewModels;
using Spinnaker.Core;

namespace Spinnaker.Sample.iPad
{
	public partial class Spinnaker_Sample_iPadViewController : UIViewController
	{
		public Spinnaker_Sample_iPadViewController (IntPtr handle) : base (handle)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			MonoTouchSpinnakerConfiguration.Init();

			SpinnakerConfiguration.CurrentConfig.LogLevel = Spinnaker.Core.SpinnakerLogLevel.Debug;

			BrowserBridge bridge = SpinnakerConfiguration.CurrentConfig.CreateBrowserBridge(webView);
			SplashViewModel splashViewModel = new SampleApplication.Core.SampleApplication().Init();
			bridge.ShowView("SplashView.html", splashViewModel);

			splashViewModel.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => 
			{
				if (e.PropertyName == "CurrentPage")
					bridge.ExecuteScriptFunction("setHeroBackground");
			};
			
			NSTimer.CreateRepeatingScheduledTimer(1.0 / 20.0, delegate
			{
				splashViewModel.RealtimeViewModel.HandleGUITimerTick();
			});
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		[Obsolete]
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return true;
		}
	}
}

