using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Blob.Test
{
	[TestClass]
	public class BlobTest : BaseTestClass
	{
		[TestMethod]
		// #TODO_BLAM: test should be renamed and made to validate all engines which support the BlobSystem
		[Description("Temp test for validating the blob system works using Reach")]
		public void Blob_BlobSystemHaloReachTest()
		{
			Engine.EngineRegistry.Initialize();

			var engines = Engine.EngineRegistry.Engines;
			var reach = Engine.EngineRegistry.EngineBranchHaloReach;

			using (var blob_system = Engine.EngineRegistry.GetSystem<BlobSystem>(reach.BranchHandle))
			{
				Assert.IsNotNull(blob_system.System);

				foreach (var g in blob_system.System.Groups)
				{
					Console.WriteLine("{0} - known-as: {1}, version-count: {2}",
						g.Key, g.Value.KnownAs, g.Value.VersionToBuildMap.Count);
				}
			}
		}
	};
}