// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace Spinnaker.Sample.iPad
{
	[Register ("Spinnaker_Sample_iPadViewController")]
	partial class Spinnaker_Sample_iPadViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIWebView webView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (webView != null) {
				webView.Dispose ();
				webView = null;
			}
		}
	}
}
