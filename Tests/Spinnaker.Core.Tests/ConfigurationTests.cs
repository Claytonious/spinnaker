using System;
using NUnit.Framework;
using Spinnaker.Bridge.Tests;
using System.IO;

namespace Spinnaker.Core.Tests
{
	[TestFixture]
	public class ConfigurationTests
	{
		[Test]
		public void TestInit()
		{
			TestSpinnakerConfiguration.Init();
			Assert.IsNotNull(SpinnakerConfiguration.CurrentConfig, "Should have successfully built a configuration but the config is null");
			Assert.IsNotNull(SpinnakerConfiguration.CurrentConfig.ViewModelManager, "Should have successfully built the singleton ViewModelManager");
		}
	}
}

