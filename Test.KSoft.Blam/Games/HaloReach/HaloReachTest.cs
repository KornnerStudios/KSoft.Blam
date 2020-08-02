using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Games.HaloReach.Test
{
	using MegaloProto = Blam.Megalo.Proto;

	[TestClass]
	public class HaloReachTest : BaseTestClass
	{
		[TestMethod]
		public void MegaloProtoSystemTest()
		{
			Engine.EngineBuildBranch halo_reach_branch = Engine.EngineRegistry.EngineBranchHaloReach;
			using (var halo_reach_megalo_proto_system_ref = Engine.EngineRegistry.GetSystem<MegaloProto.MegaloProtoSystem>(halo_reach_branch.BranchHandle))
			{
				Assert.IsTrue(halo_reach_megalo_proto_system_ref.IsValid);
				var megalo_proto_system = halo_reach_megalo_proto_system_ref.System;

				var all_dbs_tasks = megalo_proto_system.GetAllDatabasesAsync(halo_reach_branch.BranchHandle);

				System.Threading.Tasks.Task.WaitAll
					( all_dbs_tasks.Item1
					, all_dbs_tasks.Item2
					);

				Assert.IsNotNull(all_dbs_tasks.Item1.Result);
				Assert.IsNotNull(all_dbs_tasks.Item2.Result);

				MegaloProto.MegaloProtoSystem/*megalo_proto_system*/.PrepareDatabasesForUse(all_dbs_tasks.Item1.Result, all_dbs_tasks.Item2.Result);
			}
		}
	};
}
