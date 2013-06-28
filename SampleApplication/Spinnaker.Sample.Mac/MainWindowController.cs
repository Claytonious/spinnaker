
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Spinnaker.Bridge.Mac;
using Spinnaker.Sample.WinForms.ViewModels;
using Spinnaker.Core;
using Spinnaker.Log4net;

namespace Spinnaker.Sample.Mac
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		private SplashViewModel splashViewModel;

		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize();
		}
		
		// Shared initialization code
		void Initialize()
		{
		}

		public override void WindowDidLoad()
		{
			base.WindowDidLoad();

			MacSpinnakerConfiguration.Init();
			
			SpinnakerConfiguration.CurrentConfig.LogLevel = SpinnakerLogLevel.Debug;
			
			// Using log4net with Spinnaker is optional.
			log4net.Config.XmlConfigurator.Configure();
			SpinnakerLog4netAdapter.Init();

			BrowserBridge bridge = SpinnakerConfiguration.CurrentConfig.CreateBrowserBridge(webBrowser);
			splashViewModel = new SampleApplication.Core.SampleApplication().Init();
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

		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}
	}
}

