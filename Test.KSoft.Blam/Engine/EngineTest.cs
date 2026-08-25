using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Engine.Test
{
	[TestClass]
	public class EngineTest : BaseTestClass
	{
		[TestMethod]
		[Description("Validate the expected bit sizes of handles used in KSoft.Blam")]
		public void Engine_HandlesTest()
		{
			Assert.AreEqual(11, EngineBuildHandle.BitCount,
				"Expected EngineBuildHandle bit count size has changed. Was this intentional?");
			Assert.AreEqual(11+5, BlamEngineTargetHandle.BitCount,
				"Expected BlamEngineTargetHandle bit count size has changed. Was this intentional?");

			Assert.AreEqual(22, Localization.GameLanguageHandle.BitCount,
				"Expected GameLanguageHandle bit count size has changed. Was this intentional?");

			Assert.AreEqual(26, Megalo.Proto.MegaloScriptValueType.BitCount,
				"Expected MegaloScriptValueType bit count size has changed. Was this intentional?");
			Assert.AreEqual(19, Megalo.Model.MegaloScriptModelObjectHandle.BitCount,
				"Expected MegaloScriptModelObjectHandle bit count size has changed. Was this intentional?");
		}

		[TestMethod]
		public void EngineBuildHandle_TryGetValueClearsReusedValueOnMiss()
		{
			var values = new Dictionary<EngineBuildHandle, string>();
			string value = "stale";

			bool found = EngineRegistry.EngineBranchHaloReach.BranchHandle.TryGetValue(values, ref value);

			Assert.IsFalse(found);
			Assert.IsNull(value);
		}

		[TestMethod]
		[Description("Test the core engine registry for proper init and initial state")]
		public void Engine_RegistryInitializeTest()
		{
			Assert.HasCount(5, EngineRegistry.Engines,
				"Unexpected engine count in the registry. Was a new one added?");

			Assert.HasCount(5, EngineRegistry.TargetPlatforms,
				"Unexpected target platform count in the registry. Was a new one added?");

			Assert.HasCount(3, EngineRegistry.ResourceModels,
				"Unexpected resource model count in the registry. Was a new one added?");

			Assert.IsNotNull(EngineRegistry.EngineBranchHalo1);
			Assert.IsNotNull(EngineRegistry.EngineBranchHalo2);
			Assert.IsNotNull(EngineRegistry.EngineBranchHalo3);
			Assert.IsNotNull(EngineRegistry.EngineBranchHaloOdst);
			Assert.IsNotNull(EngineRegistry.EngineBranchHalo4);
			Assert.IsNotNull(EngineRegistry.EngineBranchHalo2A);
		}

		[TestMethod]
		[Description("Test that all systems are properly registered and initialized")]
		public void Engine_RegistryRegisteredSystemsTest()
		{
			Assert.HasCount(0
				+ 1 // Blob
				+ 1 // Language
				+ 1 // MegaloProto
				, EngineRegistry.Systems
				, "Unexpected engine system count in registry. Was a new one added?");

			foreach (var kv in EngineRegistry.Systems)
			{
				Assert.IsTrue(kv.Value.IsValid,
					$"Failed to properly initialize the metadata for engine system {kv.Value.EngineSystemType}. Check the debug logs.");
			}
		}

		[TestMethod]
		[Description("Test to ensure invalid and value inputs return valid strings")]
		public void GetSystemDebugDisplayStringTest()
		{
			string invalid_case_empty_display_name = EngineRegistry.GetSystemDebugDisplayString(Values.KGuid.Empty);
			Assert.IsTrue(invalid_case_empty_display_name.IsNotNullOrEmpty(), invalid_case_empty_display_name);
			Assert.AreEqual(
				"{00000000-0000-0000-0000-000000000000}=UNDEFINED_SYSTEM",
				invalid_case_empty_display_name);

			string valid_case_blob_system_display_name = EngineRegistry.GetSystemDebugDisplayString(Blob.BlobSystem.SystemGuid);
			Assert.IsTrue(valid_case_blob_system_display_name.IsNotNullOrEmpty(), valid_case_blob_system_display_name);
			Assert.AreEqual(
				"{"+Blob.BlobSystem.SystemGuid.ToString(Values.KGuid.kFormatHyphenated, KSoft.Util.InvariantCultureInfo)+"}=" + typeof(KSoft.Blam.Blob.BlobSystem),
				valid_case_blob_system_display_name);
		}
	};
}
