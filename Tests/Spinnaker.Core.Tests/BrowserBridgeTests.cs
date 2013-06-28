using System;
using NUnit.Framework;
using Spinnaker.Core;
using Spinnaker.Bridge.Tests;
using System.Collections.Generic;

namespace Spinnaker.Core.Tests
{
	[TestFixture]
	public class BrowserBridgeTests
	{
		private UnitTestWebBrowser webBrowser;
		private BrowserBridge bridge;

		[SetUp]
		public void Setup()
		{
			TestSpinnakerConfiguration.Init();
			webBrowser = new UnitTestWebBrowser();
			bridge = SpinnakerConfiguration.CurrentConfig.CreateBrowserBridge(webBrowser);
			Assert.IsNotNull(bridge, "Should have created a browser bridge");
		}

		[Test]
		public void TestShowView()
		{
			bridge.ShowView("TestView.html", new TestViewModel());
			Assert.IsNotNull(webBrowser.Content);
		}

		[Test]
		public void TestInstanceTracking ()
		{
			TestViewModel rootViewModel = new TestViewModel ();
			bridge.ShowView ("TestView.html", rootViewModel);
			int numRootViewModels = 0;
			int numChildViewModels = 0;
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (1, numRootViewModels, "Should have been aware of exactly 1 root view model");
			Assert.AreEqual (0, numChildViewModels, "Should not have been aware of ANY child view models yet");

			Assert.AreEqual (1, webBrowser.GetNumScriptFunctionInvocations ("createInstance"));
			Assert.AreEqual (0, webBrowser.GetNumScriptFunctionInvocations ("releaseInstance"));

			rootViewModel.ChildViewModel = new TestChildViewModel ();
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (1, numRootViewModels, "Should have been aware of exactly 1 root view model");
			Assert.AreEqual (1, numChildViewModels, "Should have been aware of exactly 1 child view model");
			Assert.AreEqual (2, webBrowser.GetNumScriptFunctionInvocations ("createInstance"));
			Assert.AreEqual (0, webBrowser.GetNumScriptFunctionInvocations ("releaseInstance"));

			rootViewModel.ChildViewModel = new TestChildViewModel ();
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (1, numRootViewModels, "Should have been aware of exactly 1 root view model");
			Assert.AreEqual (1, numChildViewModels, "Should have been aware of exactly 1 child view model");

			rootViewModel.ChildViewModel = null;
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (1, numRootViewModels, "Should have been aware of exactly 1 root view model");
			Assert.AreEqual (0, numChildViewModels, "Should no longer have been aware of any child view models");

			List<TestChildViewModel> childCollection = new List<TestChildViewModel> ();
			for (int i = 0; i < 100; i++)
				childCollection.Add (new TestChildViewModel ());
			rootViewModel.ChildViewModels = childCollection;
			rootViewModel.ChildViewModel = new TestChildViewModel ();
			foreach (ViewModelInstance instance in SpinnakerConfiguration.CurrentConfig.ViewModelManager.CurrentInstances)
				Assert.AreEqual (1, instance.RefCount, "Initial ref count of child should be 1");

			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (1, numRootViewModels, "Should have been aware of exactly 1 root view model");
			Assert.AreEqual (101, numChildViewModels, "Should have been aware of all child view models");

			childCollection.RemoveRange (0, 50);
			rootViewModel.ChildViewModels = childCollection;
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (1, numRootViewModels, "Should have been aware of exactly 1 root view model");
			Assert.AreEqual (51, numChildViewModels, "Should have let go of 50 child view models");
			foreach (ViewModelInstance instance in SpinnakerConfiguration.CurrentConfig.ViewModelManager.CurrentInstances)
				Assert.AreEqual (1, instance.RefCount, "Ref count of child should still be 1");

			childCollection.Add (rootViewModel.ChildViewModel);
			rootViewModel.ChildViewModels = childCollection;
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (51, numChildViewModels, "Should have realized a collection member and a root member were the same view model");
			rootViewModel.ChildViewModel = null;
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (51, numChildViewModels, "Should have realized a collection member and a root member were the same view model");
			childCollection.RemoveAt (childCollection.Count - 1);
			rootViewModel.ChildViewModels = childCollection;
			CountInstances (out numRootViewModels, out numChildViewModels);
			Assert.AreEqual (1, numRootViewModels, "Should have been aware of exactly 1 root view model");
			Assert.AreEqual (50, numChildViewModels, "Should have released child via ref counting");

			foreach (ViewModelInstance instance in SpinnakerConfiguration.CurrentConfig.ViewModelManager.CurrentInstances) 
				Assert.AreEqual (1, instance.RefCount, "Ref count of child should be 1");
		}

		private void CountInstances(out int numRoots, out int numChild)
		{
			numRoots = numChild = 0;
			foreach (ViewModelInstance instance in SpinnakerConfiguration.CurrentConfig.ViewModelManager.CurrentInstances) 
			{
				Assert.IsNotNull(instance.ViewModel);
				if (instance.ViewModel is TestViewModel)
					numRoots++;
				else if (instance.ViewModel is TestChildViewModel)
					numChild++;
			}
		}
	}
}

