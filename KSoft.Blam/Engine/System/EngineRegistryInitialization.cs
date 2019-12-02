using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KSoft.Blam.Engine
{
	partial class EngineRegistry
	{
		static void InitializePrototypes()
		{
			// #TODO_BLAM: if we use a 'null' engine entry, add it here, before the prototypes are read
			using (var s = OpenRegistryTagElementStream())
			{
				SerializePrototypes(s);
				SerializeTargets(s);
			}

			KSoft.Debug.ValueCheck.IsLessThanEqualTo("EngineRegistry: Too many registered engines",
				BlamEngine.kMaxCount, Engines.Count);
			KSoft.Debug.ValueCheck.IsLessThanEqualTo("EngineRegistry: Too many registered target platforms",
				EngineTargetPlatform.kMaxCount, TargetPlatforms.Count);
			KSoft.Debug.ValueCheck.IsLessThanEqualTo("EngineRegistry: Too many registered resource models",
				kMaxResourceModels, ResourceModels.Count);

			KSoft.Debug.ValueCheck.IsDistinct("EngineRegistry: Duplicate engines registered",
				"name", Engines.Select(e => e.Name));
			KSoft.Debug.ValueCheck.IsDistinct("EngineRegistry: Duplicate target platforms registered",
				"name", TargetPlatforms.Select(e => e.Name));
			KSoft.Debug.ValueCheck.IsDistinct("EngineRegistry: Duplicate resource models registered",
				"name", ResourceModels);

			gEngines.TrimExcess();
			gTargetPlatforms.TrimExcess();
			gResourceModels.TrimExcess();

			BlamEngine.InitializeEngineBuildHandles();
			kNullValidTargetPlatforms = new Collections.BitSet(TargetPlatforms.Count);
		}
		static void InitializeEngines()
		{
			foreach (var e in gEngines)
			{
				using (var s = OpenEngineTagElementStream(e))
					e.Serialize(s);
			}

			BlamEngine.InitializeEngineRepositoryBuildHandles();
		}
		static void InitializeExportedBuilds()
		{
			gExportedBuildsByName = new Dictionary<string, EngineBuildRevision>();

			foreach (var r in
				from e in Engines
				from b in e.BuildRepository.Branches
				from r in b.Revisions
				where !string.IsNullOrEmpty(r.ExportName)
				select r)
			{
				if (gExportedBuildsByName.ContainsKey(r.ExportName))
					throw new InvalidDataException(string.Format(
						"build={0} tried to export with a name that is already in use: {1}",
						r.BuildHandle.ToDisplayString(), r.ExportName));

				gExportedBuildsByName.Add(r.ExportName, r);
			}
		}

		static void ResolveWellKnownEngines()
		{
			EngineBranchHalo1 = BlamEngine.ResolveWellKnownEngineBranch("Halo1", "Halo1");
			EngineBranchHalo2 = BlamEngine.ResolveWellKnownEngineBranch("Halo2", "Halo2");
			EngineBranchHalo3 = BlamEngine.ResolveWellKnownEngineBranch("Halo3", "Halo3");
			EngineBranchHaloOdst = BlamEngine.ResolveWellKnownEngineBranch("Halo3", "HaloOdst");
			EngineBranchHaloReach = BlamEngine.ResolveWellKnownEngineBranch("HaloReach", "HaloReach");
			EngineBranchHalo4 = BlamEngine.ResolveWellKnownEngineBranch("Halo4", "Halo4");
		}

		public static void Initialize()
		{
			gEngines = new List<BlamEngine>(BlamEngine.kMaxCount);
			gTargetPlatforms = new List<EngineTargetPlatform>(EngineTargetPlatform.kMaxCount);
			gResourceModels = new List<string>(kMaxResourceModels);

			InitializePrototypes();
			InitializeEngines();
			InitializeExportedBuilds();
			ResolveWellKnownEngines();
		}
		public static void Dispose()
		{
			gEngines = null;
			gTargetPlatforms = null;
			kNullValidTargetPlatforms = null;
			gResourceModels = null;
			gExportedBuildsByName = null;

			gSystems = null;

			EngineBranchHalo1 =
				EngineBranchHalo2 =
				EngineBranchHalo3 = EngineBranchHaloOdst =
				EngineBranchHaloReach =
				EngineBranchHalo4 =
				null;
		}

		internal static void InitializeForNewProgram()
		{
			EngineSystemAttribute.InitializeForNewAssembly(System.Reflection.Assembly.GetCallingAssembly());
		}
		internal static void DisposeFromOldProgram()
		{
		}
	};
}