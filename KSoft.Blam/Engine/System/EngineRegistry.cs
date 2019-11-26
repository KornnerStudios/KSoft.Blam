using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
namespace KSoft.Blam.Engine
{
	/// <summary>Tracks all global-level engine information and objects</summary>
	public static partial class EngineRegistry
	{
		const string kErrorMessageNotInitialized =
			"EngineRegistry not yet initialized";

		const string kRegistryFilePath = @"Engine\EngineRegistry.xml";
		const string kSeriesName = "Blam";

		#region Engines
		static List<BlamEngine> gEngines;
		public static IReadOnlyList<BlamEngine> Engines { get {
			Contract.Assert(gEngines != null, kErrorMessageNotInitialized);

			return gEngines;
		} }

		public static EngineBuildBranch EngineBranchHalo1 { get; private set; }
		public static EngineBuildBranch EngineBranchHalo2 { get; private set; }
		public static EngineBuildBranch EngineBranchHalo3 { get; private set; }
		public static EngineBuildBranch EngineBranchHaloOdst { get; private set; }
		public static EngineBuildBranch EngineBranchHaloReach { get; private set; }
		public static EngineBuildBranch EngineBranchHalo4 { get; private set; }
		#endregion

		#region TargetPlatforms
		static List<EngineTargetPlatform> gTargetPlatforms;
		public static IReadOnlyList<EngineTargetPlatform> TargetPlatforms { get {
			Contract.Assert(gTargetPlatforms != null, kErrorMessageNotInitialized);

			return gTargetPlatforms;
		} }

		static Collections.IReadOnlyBitSet kNullValidTargetPlatforms;
		/// <summary>Represents a BitSet of ValidTargetPlatforms that are all set to false</summary>
		internal static Collections.IReadOnlyBitSet NullValidTargetPlatforms { get {
			Contract.Assert(kNullValidTargetPlatforms != null, kErrorMessageNotInitialized);

			return kNullValidTargetPlatforms;
		} }
		#endregion

		#region ResourceModels
		internal const int kMaxResourceModels = 4; // 2 bits
		internal static readonly int kResourceModelBitCount = Bits.GetMaxEnumBits(kMaxResourceModels);
		private static readonly uint kResourceModelBitMask = Bits.BitCountToMask32(kResourceModelBitCount);

		static List<string> gResourceModels;
		public static IReadOnlyList<string> ResourceModels { get {
			Contract.Assert(gResourceModels != null, kErrorMessageNotInitialized);

			return gResourceModels;
		} }

		[Contracts.Pure]
		[System.Diagnostics.DebuggerStepThrough]
		public static bool IsValidResourceModelIndex(int resourceModelIndex)
		{
			return resourceModelIndex.IsNoneOrPositive() && resourceModelIndex < ResourceModels.Count;
		}

		static int ResourceModelIdResolver(object _null, string name)
		{
			int id = TypeExtensions.kNone;

			if (!string.IsNullOrEmpty(name))
			{
				id = ResourceModels.FindIndex(x => name.Equals(x));

				if (id.IsNone())
					throw new KeyNotFoundException(string.Format("No resource model is registered with the name '{0}'",
						name));
			}

			return id;
		}
		static readonly Func<object, string, int> ResourceModelIdResolverSansKeyNotFoundException =
			(_null, name) => !string.IsNullOrEmpty(name)
				? ResourceModels.FindIndex(x => name.Equals(x))
				: TypeExtensions.kNone;
		static readonly Func<object, int, string> ResourceModelNameResolver =
			(_null, id) => id.IsNotNone()
				? ResourceModels[id]
				: null;
		#endregion

		#region Exported Builds
		static Dictionary<string, EngineBuildRevision> gExportedBuildsByName;
		public static IReadOnlyDictionary<string, EngineBuildRevision> ExportedBuildsByName { get {
			Contract.Assert(gExportedBuildsByName != null, kErrorMessageNotInitialized);

			return gExportedBuildsByName;
		} }
		#endregion

		#region Systems
		static Dictionary<Values.KGuid, EngineSystemAttribute> gSystems =
			new Dictionary<Values.KGuid, EngineSystemAttribute>();
		internal static IReadOnlyDictionary<Values.KGuid, EngineSystemAttribute> Systems { get {
			return gSystems;
		} }

		/// <summary>Don't call me unless your name is <see cref="EngineSystemAttribute"/>!</summary>
		/// <param name="systemMetadata">	The system metadata. </param>
		internal static void Register(EngineSystemAttribute systemMetadata)
		{
			Contract.Requires(systemMetadata != null);
			// IReadOnlyDictionary's ContainsKey is not Pure. Check is instead performed in EngineSystemAttribute code
			//Contract.Requires(!Systems.ContainsKey(systemMetadata.Guid));

			gSystems.Add(systemMetadata.Guid, systemMetadata);
		}

		/// <summary>Try to get the metadata for an <see cref="EngineSystemBase"/> via its guid</summary>
		/// <param name="systemGuid"></param>
		/// <returns>Null if no system is registered with the provided guid</returns>
		public static EngineSystemAttribute TryGetRegisteredSystem(Values.KGuid systemGuid)
		{
			Contract.Requires<ArgumentNullException>(systemGuid != Values.KGuid.Empty);

			EngineSystemAttribute metadata;
			Systems.TryGetValue(systemGuid, out metadata);

			return metadata;
		}

		/// <summary>Get an <see cref="EngineSystemBase"/> associated with a registered engine</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="forBuild">The build requesting this system</param>
		/// <returns></returns>
		public static EngineSystemReference<T> GetSystem<T>(EngineBuildHandle forBuild)
			where T : EngineSystemBase
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);

			var engine = Engines[forBuild.EngineIndex];

			return engine.GetSystem<T>(forBuild);
		}
		/// <summary>Tries to get an <see cref="EngineSystemBase"/> associated with a registered engine</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="forBuild">The build requesting this system</param>
		/// <returns><see cref="EngineSystemReference{T}.None"/> if the engine doesn't support the requested system</returns>
		public static EngineSystemReference<T> TryGetSystem<T>(EngineBuildHandle forBuild)
			where T : EngineSystemBase
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);

			var engine = Engines[forBuild.EngineIndex];

			return engine.TryGetSystem<T>(forBuild);
		}
		#endregion

		#region Initialization
		static void InitializePrototypes()
		{
			// TODO: if we use a 'null' engine entry, add it here, before the prototypes are read
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

			foreach(var e in Engines)
				foreach(var b in e.BuildRepository.Branches)
					foreach(var r in b.Revisions)
					{
						if (string.IsNullOrEmpty(r.ExportName))
							continue;

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
		#endregion

		#region Bit encoding
		internal static void BitEncodeResourceModelIndex(ref Bitwise.HandleBitEncoder encoder, int resourceModelIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(IsValidResourceModelIndex(resourceModelIndex));

			encoder.EncodeNoneable32(resourceModelIndex, kResourceModelBitMask);
		}
		internal static int BitDecodeResourceModelIndex(uint handle, int bitIndex)
		{
			var index = Bits.BitDecodeNoneable(handle, bitIndex, kResourceModelBitMask);

			Contract.Assert(IsValidResourceModelIndex(index));
			return index;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		static dynamic OpenRegistryTagElementStream(FileAccess streamMode = FileAccess.Read)
		{
			Contract.Requires<FileNotFoundException>(File.Exists(kRegistryFilePath),
				"Can't initialize the EngineRegistry, need the following file: " + kRegistryFilePath);

			var stream = IO.TagElementStreamFactory.Open(kRegistryFilePath);
			stream.StreamMode = streamMode;

			return stream;
		}

		static dynamic OpenEngineTagElementStream(BlamEngine engine, FileAccess streamMode = FileAccess.Read)
		{
			Contract.Assert(engine != null);

			string engine_stream_path = Path.Combine(@"Games\", engine.Name, engine.Name);
			engine_stream_path += ".xml";

			if (!File.Exists(engine_stream_path))
				throw new FileNotFoundException(string.Format(
					"Can't initialize the {0} engine, need the following file: {1}",
					engine.Name,
					engine_stream_path));

			var stream = IO.TagElementStreamFactory.Open(engine_stream_path);
			stream.StreamMode = streamMode;

			return stream;
		}
		static dynamic OpenEngineSystemTagElementStream(BlamEngine engine, Values.KGuid systemGuid, string externFileName,
			FileAccess streamMode = FileAccess.Read)
		{
			Contract.Assert(engine != null);

			string extern_stream_path = Path.Combine(@"Games\", engine.Name, externFileName);

			if (!File.Exists(extern_stream_path))
				throw new FileNotFoundException(string.Format(
					"Can't initialize the {0} engine's {1} system, need the following file: {2}",
					engine.Name,
					systemGuid.ToString(Values.KGuid.kFormatHyphenated),
					extern_stream_path));

			var stream = IO.TagElementStreamFactory.Open(extern_stream_path);
			stream.StreamMode = streamMode;

			return stream;
		}

		static void SerializePrototypes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// NOTE: we don't current validate the series name
			string series = kSeriesName;
			s.StreamAttribute("series", ref series);

			using (var bm = s.EnterCursorBookmarkOpt("Engines", gEngines, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamElements("Engine", gEngines, (object)null,
					BlamEngine.SerializePrototype);
		}
		static void SerializeTargets<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Targets"))
			{
				using (var bm = s.EnterCursorBookmarkOpt("Platforms", gTargetPlatforms, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Platform", gTargetPlatforms);

				using (var bm = s.EnterCursorBookmarkOpt("ResourceModels", gResourceModels, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamElements("Model", gResourceModels);
			}
		}

		internal static bool SerializeResourceModelId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int resourceModelId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref resourceModelId, null,
					ResourceModelIdResolver, ResourceModelNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
					resourceModelId = TypeExtensions.kNone;
			}
			else
			{
				s.StreamAttributeIdAsString(attributeName, ref resourceModelId, null,
					ResourceModelIdResolver, ResourceModelNameResolver);
			}

			return streamed;
		}
		#endregion
	};
}