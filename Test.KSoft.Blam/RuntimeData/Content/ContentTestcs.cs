using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.RuntimeData.Test
{
	using EngineBuildBranchAndStringKvp = KeyValuePair<Engine.EngineBuildBranch, string>;

	[TestClass]
	public class ContentTest : BaseTestClass
	{
		[ClassInitialize]
		public static void ContentTestInitialize(TestContext testContext)
		{
			Engine.EngineRegistry.Initialize();
		}
		[ClassCleanup]
		public static void ContentTestDispose()
		{
			Engine.EngineRegistry.Dispose();
		}

		#region ContentMiniMetadataEncoding
		static void TestContentMiniMetadataEncoding(EngineBuildBranchAndStringKvp kv)
		{
 			Console.WriteLine(kv.Value);
 			var metadata = ContentMiniMetadata.Decode(kv.Key.BranchHandle, kv.Value);
 			var s = Console.Out;
			using (var xml = IO.XmlElementStream.CreateForWrite("ContentMiniMetadata"))
 			{
 				metadata.Serialize(xml);
				xml.Document.Save(s);
				s.WriteLine();
 			}
 			string encoded = metadata.Encode();
 			Console.WriteLine(encoded);
 			Assert.AreEqual(kv.Value, encoded);
 			Console.WriteLine();
		}
		[TestMethod]
		[Description("Really basic test to verify decode then re-encode is working")]
		public void ContentMiniMetadataEncodingTest()
		{
			EngineBuildBranchAndStringKvp[] kContainerNames =
			{
				new EngineBuildBranchAndStringKvp(Engine.EngineRegistry.EngineBranchHalo4, "gfyccqramf3545555m2exv3nst10wqyfqaeaaaaaa"), // oddball
				new EngineBuildBranchAndStringKvp(Engine.EngineRegistry.EngineBranchHalo4, "gfyccaja0fz545555imwszqgtcysgxrmv12ihzebo"), // ctf
			};

			foreach (var kv in kContainerNames)
 				TestContentMiniMetadataEncoding(kv);
		}
		#endregion
	};
}