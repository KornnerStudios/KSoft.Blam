using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Engine.Test
{
	[TestClass]
	public class EngineTest : BaseTestClass
	{
		[TestMethod]
		public void Engine_RegistryInitializeTest()
		{
			EngineRegistry.Initialize();

			Assert.AreEqual(5, EngineRegistry.Engines.Count,
				"Unexpected engine count in the registry. Was a new one added?");

			Assert.AreEqual(5, EngineRegistry.TargetPlatforms.Count,
				"Unexpected target platform count in the registry. Was a new one added?");

			Assert.AreEqual(3, EngineRegistry.ResourceModels.Count,
				"Unexpected resource model count in the registry. Was a new one added?");

			Assert.IsNotNull(EngineRegistry.EngineBranchHalo1);
			Assert.IsNotNull(EngineRegistry.EngineBranchHalo2);
			Assert.IsNotNull(EngineRegistry.EngineBranchHalo3);
			Assert.IsNotNull(EngineRegistry.EngineBranchHaloOdst);
			Assert.IsNotNull(EngineRegistry.EngineBranchHalo4);
		}

		[TestMethod]
		public void Engine_RegistryRegisteredSystemsTest()
		{
			Assert.AreEqual(0
				+ 1 // Blob
				+ 1 // Language
				+ 1 // MegaloProto
				, EngineRegistry.Systems.Count
				, "Unexpected engine system count in registry. Was a new one added?");

			foreach(var kv in EngineRegistry.Systems)
			{
				Assert.IsTrue(kv.Value.IsValid,
					"Failed to properly initialize the metadata for engine system {0}. Check the debug logs.",
					kv.Value.EngineSystemType);
			}
		}
	};
}