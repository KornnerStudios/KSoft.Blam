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

				var megalo_static_db_task = megalo_proto_system.GetStaticDatabaseAsync(halo_reach_branch.BranchHandle);
				var megalo_script_db_task = megalo_proto_system.GetMegaloDatabaseAsync(halo_reach_branch.BranchHandle);

				System.Threading.Tasks.Task.WaitAll
					( megalo_static_db_task
					, megalo_script_db_task
					);

				Assert.IsNotNull(megalo_static_db_task.Result);
				Assert.IsNotNull(megalo_script_db_task.Result);
			}
		}
	};
}