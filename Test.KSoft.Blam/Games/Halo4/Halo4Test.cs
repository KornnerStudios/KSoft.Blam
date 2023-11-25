using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Games.Halo4.Test
{
	using MegaloProto = Blam.Megalo.Proto;
	using BSE = Blam.TypeExtensionsBlam;

	[TestClass]
	public class Halo4Test : BaseTestClass
	{
		[TestMethod]
		public void MegaloProtoSystemTest()
		{
			Engine.EngineBuildBranch halo4_branch = Engine.EngineRegistry.EngineBranchHalo4;
			using (var halo4_megalo_proto_system_ref = Engine.EngineRegistry.GetSystem<MegaloProto.MegaloProtoSystem>(halo4_branch.BranchHandle))
			{
				Assert.IsTrue(halo4_megalo_proto_system_ref.IsValid);
				var megalo_proto_system = halo4_megalo_proto_system_ref.System;

				var all_dbs_tasks = megalo_proto_system.GetAllDatabasesAsync(halo4_branch.BranchHandle);

				System.Threading.Tasks.Task.WaitAll
					( all_dbs_tasks.Item1
					, all_dbs_tasks.Item2
					);

				Assert.IsNotNull(all_dbs_tasks.Item1.Result);
				Assert.IsNotNull(all_dbs_tasks.Item2.Result);

				MegaloProto.MegaloProtoSystem/*megalo_proto_system*/.PrepareDatabasesForUse(all_dbs_tasks.Item1.Result, all_dbs_tasks.Item2.Result);
			}
		}

		/// <summary>
		/// Some cases in GameBitStreamSingleEncodingTest do not come out exactly equal when decoded in Release.
		/// So make sure they're at least close enough.
		/// </summary>
		/// <param name="expectedFloat"></param>
		/// <param name="actualFloat"></param>
		void AssertCompareFloats(float expectedFloat, float actualFloat)
		{
			// After updating to VS2022, things started coming out exactly equal, go figure
#if true//DEBUG
			Assert.AreEqual(expectedFloat, actualFloat);
#else
			if (expectedFloat != actualFloat)
			{
				float abs = System.Math.Abs(expectedFloat - actualFloat);
				Assert.IsTrue(abs > float.Epsilon, $"expected: {expectedFloat}, actual: {actualFloat}, absolute diff: {abs}");
			}
#endif
		}

		[TestMethod]
		public void GameBitStreamSingleEncodingTest()
		{
			#region values assumed from blind reverse engineering
			const float k_expected_float1 = 2.00204468f;
			const uint k_expected_float1_bits = 0x8147;

			float result_float = BSE.DecodeSingle(k_expected_float1_bits, -200.0f, 200.0f, 16, true, true);
			AssertCompareFloats(k_expected_float1, result_float);
			uint result_bits = BSE.EncodeSingle(result_float, -200.0f, 200.0f, 16, true, true);
			Assert.AreEqual(k_expected_float1_bits, result_bits);

			const float k_expected_float2 = 1.50152588f;
			const uint k_expected_float2_bits = 0x80F5;

			result_float = BSE.DecodeSingle(k_expected_float2_bits, -200.0f, 200.0f, 16, true, true);
			AssertCompareFloats(k_expected_float2, result_float);
			result_bits = BSE.EncodeSingle(result_float, -200.0f, 200.0f, 16, true, true);
			Assert.AreEqual(k_expected_float2_bits, result_bits);
			#endregion

			#region values taken from debug session
			int k_bit_count = 16;
			float k_min = -2.00000000000000E+002f;
			float k_max = +2.00000000000000E+002f;
			float input; uint output;

			#region signed
			input = +1.45269775390625E+000f;
			output = 0x80ED;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			AssertCompareFloats(input, result_float);

			input = +1.59919738769531E+000f;
			output = 0x8105;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			AssertCompareFloats(input, result_float);

			input = +1.75178527832031E+000f;
			output = 0x811E;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			AssertCompareFloats(input, result_float);

			input = +1.50152587890625E+000f;
			output = 0x80F5;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			AssertCompareFloats(input, result_float);

			input = +2.00204467773438E+000f;
			output = 0x8147;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			Assert.AreEqual(input, result_float);
			#endregion

			#region unsigned
			k_bit_count = 30;
			k_min = +0.00000000000000E+000f;
			k_max = +1.00000000000000E+004f;

			input = +1.00000298023224E+000f;
			output = 0x1A36F;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, false, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, false, true);
			Assert.AreEqual(output, result_bits);
			Assert.AreEqual(input, result_float);

			input = +7.00000076293945E+001f;
			output = 0x72B022;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, false, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, false, true);
			Assert.AreEqual(output, result_bits);
			Assert.AreEqual(input, result_float);

			input = +3.00002276897430E-001f;
			output = 0x7DD5;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, false, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, false, true);
			Assert.AreEqual(output, result_bits);
			Assert.AreEqual(input, result_float);
			#endregion
			#endregion

			k_bit_count = 20;
			k_min = 0f;
			k_max = 1000f;

			input = 14.000456f;
			output = 0x3959;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			Assert.AreEqual(input, result_float);

			input = 11.999641f;
			output = 0x3127;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			Assert.AreEqual(input, result_float);


			input = 9.9997807f;
			output = 0x28F6;
			result_bits = BSE.EncodeSingle(input, k_min, k_max, k_bit_count, true, true);
			result_float = BSE.DecodeSingle(output, k_min, k_max, k_bit_count, true, true);
			Assert.AreEqual(output, result_bits);
			Assert.AreEqual(input, result_float);
		}

		void TestMegaloVariantFromRetailXbox360(Blam.RuntimeData.Variants.GameEngineMegaloVariant megalo_variant)
		{
			#region CTF
			if (megalo_variant.BaseVariant.Header.Title == "Capture the Flag")
			{
				Assert.AreEqual(0xC3, megalo_variant.EngineDefinition.Conditions.Count);
				Assert.AreEqual(0x198, megalo_variant.EngineDefinition.Actions.Count);
				Assert.AreEqual(0x47, megalo_variant.EngineDefinition.Triggers.Count);
				Assert.AreEqual(4, megalo_variant.EngineDefinition.GameStatistics.Count);
				Assert.AreEqual(2, megalo_variant.EngineDefinition.HudWidgets.Count);
				Assert.AreEqual(6, megalo_variant.EngineDefinition.ObjectFilters.Count);
				Assert.AreEqual(0, megalo_variant.EngineDefinition.CandySpawnerFilters.Count);
			}
			#endregion
			#region Dominion
			else if (megalo_variant.BaseVariant.Header.Title == "Dominion")
			{
				Assert.AreEqual(0x19A, megalo_variant.EngineDefinition.Conditions.Count);
				Assert.AreEqual(0x3D7, megalo_variant.EngineDefinition.Actions.Count);
				Assert.AreEqual(0x7B, megalo_variant.EngineDefinition.Triggers.Count);
				Assert.AreEqual(4, megalo_variant.EngineDefinition.GameStatistics.Count);
				Assert.AreEqual(0, megalo_variant.EngineDefinition.HudWidgets.Count);
				Assert.AreEqual(0xD, megalo_variant.EngineDefinition.ObjectFilters.Count);
				Assert.AreEqual(2, megalo_variant.EngineDefinition.CandySpawnerFilters.Count);
			}
			#endregion
			#region Flood
			else if (megalo_variant.BaseVariant.Header.Title == "Flood")
			{
				Assert.AreEqual(0xB7, megalo_variant.EngineDefinition.Conditions.Count);
				Assert.AreEqual(0x155, megalo_variant.EngineDefinition.Actions.Count);
				Assert.AreEqual(0x23, megalo_variant.EngineDefinition.Triggers.Count);
				Assert.AreEqual(4, megalo_variant.EngineDefinition.GameStatistics.Count);
				Assert.AreEqual(0, megalo_variant.EngineDefinition.HudWidgets.Count);
				Assert.AreEqual(3, megalo_variant.EngineDefinition.ObjectFilters.Count);
				Assert.AreEqual(0, megalo_variant.EngineDefinition.CandySpawnerFilters.Count);
			}
			#endregion
			#region Team Regicide
			else if (megalo_variant.BaseVariant.Header.Title == "Team Regicide")
			{
				Assert.AreEqual(0x99, megalo_variant.EngineDefinition.Conditions.Count);
				Assert.AreEqual(0x130, megalo_variant.EngineDefinition.Actions.Count);
				Assert.AreEqual(0x18, megalo_variant.EngineDefinition.Triggers.Count);
				Assert.AreEqual(4, megalo_variant.EngineDefinition.GameStatistics.Count);
				Assert.AreEqual(0, megalo_variant.EngineDefinition.HudWidgets.Count);
				Assert.AreEqual(3, megalo_variant.EngineDefinition.ObjectFilters.Count);
				Assert.AreEqual(0, megalo_variant.EngineDefinition.CandySpawnerFilters.Count);
			}
			#endregion
			#region Oddball
			else if (megalo_variant.BaseVariant.Header.Title == "Oddball")
			{
				Assert.AreEqual(0xA3, megalo_variant.EngineDefinition.Conditions.Count);
				Assert.AreEqual(0x183, megalo_variant.EngineDefinition.Actions.Count);
				Assert.AreEqual(0x27, megalo_variant.EngineDefinition.Triggers.Count);
				Assert.AreEqual(4, megalo_variant.EngineDefinition.GameStatistics.Count);
				Assert.AreEqual(0, megalo_variant.EngineDefinition.HudWidgets.Count);
				Assert.AreEqual(6, megalo_variant.EngineDefinition.ObjectFilters.Count);
				Assert.AreEqual(1, megalo_variant.EngineDefinition.CandySpawnerFilters.Count);
			}
			#endregion
		}
	};
}
