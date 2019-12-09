using System;
using System.Collections.Generic;
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
		public static EngineBuildBranch EngineBranchHalo2A { get; private set; }

		public static EngineBuildHandle TryParseEngineBranchName(string branchNameToFind)
		{
			var found_handle = EngineBuildHandle.None;

			if (branchNameToFind.IsNotNullOrEmpty())
			{
				foreach (var engine in gEngines)
				{
					foreach (var branch in engine.BuildRepository.Branches)
					{
						if (string.Compare(branch.Name, branchNameToFind, StringComparison.OrdinalIgnoreCase)==0)
						{
							found_handle = branch.BranchHandle;
							goto exit;
						}
					}
				}
			}

		exit:
			return found_handle;
		}
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

		public static EngineBuildRevision TryParseExportedBuildName(string exportedNameToFind)
		{
			EngineBuildRevision found_revision = null;

			if (exportedNameToFind.IsNotNullOrEmpty())
			{
				foreach (var kvp in ExportedBuildsByName)
				{
					if (string.Compare(kvp.Key, exportedNameToFind, StringComparison.OrdinalIgnoreCase)==0)
					{
						found_revision = kvp.Value;
						break;
					}
				}
			}

			return found_revision;
		}
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
			Contract.Requires<ArgumentNullException>(systemGuid.IsNotEmpty);

			EngineSystemAttribute metadata;
			Systems.TryGetValue(systemGuid, out metadata);

			return metadata;
		}

		/// <summary>Get a human readable display string for debugging system references from a GUID</summary>
		/// <param name="systemGuid"></param>
		/// <returns>Non-null or empty string, no matter the input</returns>
		public static string GetSystemDebugDisplayString(Values.KGuid systemGuid)
		{
			Contract.Ensures(Contract.Result<string>().IsNotNullOrEmpty());

			EngineSystemAttribute system_attribute = null;
			if (systemGuid.IsNotEmpty)
				system_attribute = TryGetRegisteredSystem(systemGuid);

			string display_string = string.Format("{{{0}}}={1}",
				systemGuid.ToString(Values.KGuid.kFormatHyphenated),
				system_attribute != null
					? system_attribute.EngineSystemType.ToString()
					: "UNDEFINED_SYSTEM");

			return display_string;
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
	};
}