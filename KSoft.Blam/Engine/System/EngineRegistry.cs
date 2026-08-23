using System;
using System.Collections.Generic;

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
		static List<BlamEngine> gEngines = null!;
		public static IReadOnlyList<BlamEngine> Engines { get {
			if (gEngines == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return gEngines;
		} }

		public static bool IsInitialized { get { return gEngines != null; } }

		public static EngineBuildBranch EngineBranchHalo1 { get; private set; } = null!;
		public static EngineBuildBranch EngineBranchHalo2 { get; private set; } = null!;
		public static EngineBuildBranch EngineBranchHalo3 { get; private set; } = null!;
		public static EngineBuildBranch EngineBranchHaloOdst { get; private set; } = null!;
		public static EngineBuildBranch EngineBranchHaloReach { get; private set; } = null!;
		public static EngineBuildBranch EngineBranchHalo4 { get; private set; } = null!;
		public static EngineBuildBranch EngineBranchHalo2A { get; private set; } = null!;

		public static EngineBuildHandle TryParseEngineBranchName(string branchNameToFind)
		{
			var found_handle = EngineBuildHandle.None;

			if (branchNameToFind.IsNotNullOrEmpty())
			{
				foreach (var engine in Engines)
				{
					foreach (var branch in engine.BuildRepository.Branches)
					{
						if (string.Equals(branch.Name, branchNameToFind, StringComparison.OrdinalIgnoreCase))
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
		static List<EngineTargetPlatform> gTargetPlatforms = null!;
		public static IReadOnlyList<EngineTargetPlatform> TargetPlatforms { get {
			if (gTargetPlatforms == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return gTargetPlatforms;
		} }

		static Collections.IReadOnlyBitSet kNullValidTargetPlatforms = null!;
		/// <summary>Represents a BitSet of ValidTargetPlatforms that are all set to false</summary>
		internal static Collections.IReadOnlyBitSet NullValidTargetPlatforms { get {
			if (kNullValidTargetPlatforms == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return kNullValidTargetPlatforms;
		} }
		#endregion

		#region ResourceModels
		internal const int kMaxResourceModels = 4; // 2 bits
		internal static readonly int kResourceModelBitCount = Bits.GetMaxEnumBits(kMaxResourceModels);
		private static readonly uint kResourceModelBitMask = Bits.BitCountToMask32(kResourceModelBitCount);

		static List<string> gResourceModels = null!;
		public static IReadOnlyList<string> ResourceModels { get {
			if (gResourceModels == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return gResourceModels;
		} }

		[System.Diagnostics.DebuggerStepThrough]
		public static bool IsValidResourceModelIndex(int resourceModelIndex)
		{
			return resourceModelIndex.IsNoneOrPositive() && resourceModelIndex < ResourceModels.Count;
		}

		static int ResourceModelIdResolver(object? _null, string name)
		{
			int id = TypeExtensions.kNone;

			if (!string.IsNullOrEmpty(name))
			{
				id = ResourceModels.FindIndex(x => name.Equals(x, StringComparison.Ordinal));

				if (id.IsNone())
				{
					throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
						"No resource model is registered with the name '{0}'",
						name));
				}
			}

			return id;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		static readonly Func<object, string, int> ResourceModelIdResolverSansKeyNotFoundException =
			(_null, name) => !string.IsNullOrEmpty(name)
				? ResourceModels.FindIndex(x => name.Equals(x, StringComparison.Ordinal))
				: TypeExtensions.kNone;
		static readonly Func<object?, int, string> ResourceModelNameResolver =
			(_null, id) => id.IsNotNone()
				? ResourceModels[id]
				: null!;
		#endregion

		#region Exported Builds
		static Dictionary<string, EngineBuildRevision> gExportedBuildsByName = null!;
		public static IReadOnlyDictionary<string, EngineBuildRevision> ExportedBuildsByName { get {
			if (gExportedBuildsByName == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return gExportedBuildsByName;
		} }

		public static EngineBuildRevision? TryParseExportedBuildName(string exportedNameToFind)
		{
			EngineBuildRevision? found_revision = null;

			if (exportedNameToFind.IsNotNullOrEmpty())
			{
				foreach (var kvp in ExportedBuildsByName)
				{
					if (string.Equals(kvp.Key, exportedNameToFind, StringComparison.OrdinalIgnoreCase))
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
		static Dictionary<Values.KGuid, EngineSystemAttribute> gSystems = new();
		internal static IReadOnlyDictionary<Values.KGuid, EngineSystemAttribute> Systems => gSystems;

		/// <summary>Don't call me unless your name is <see cref="EngineSystemAttribute"/>!</summary>
		/// <param name="systemMetadata">	The system metadata. </param>
		internal static void Register(EngineSystemAttribute systemMetadata)
		{
			ArgumentNullException.ThrowIfNull(systemMetadata);

			gSystems.Add(systemMetadata.SystemGuid, systemMetadata);
		}

		/// <summary>Try to get the metadata for an <see cref="EngineSystemBase"/> via its guid</summary>
		/// <param name="systemGuid"></param>
		/// <returns>Null if no system is registered with the provided guid</returns>
		public static EngineSystemAttribute? TryGetRegisteredSystem(Values.KGuid systemGuid)
		{
			if (systemGuid.IsEmpty)
			{
				throw new ArgumentException("System GUID must not be empty.", nameof(systemGuid));
			}

			Systems.TryGetValue(systemGuid, out EngineSystemAttribute? metadata);

			return metadata;
		}

		/// <summary>Get a human readable display string for debugging system references from a GUID</summary>
		/// <param name="systemGuid"></param>
		/// <returns>Non-null or empty string, no matter the input</returns>
		public static string GetSystemDebugDisplayString(Values.KGuid systemGuid)
		{
			EngineSystemAttribute? system_attribute = null;
			if (systemGuid.IsNotEmpty)
			{
				system_attribute = TryGetRegisteredSystem(systemGuid);
			}

			string display_string = string.Format(Util.InvariantCultureInfo,
				"{{{0}}}={1}",
				systemGuid.ToString(Values.KGuid.kFormatHyphenated, Util.InvariantCultureInfo),
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
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			BlamEngine engine = Engines[forBuild.EngineIndex];

			return engine.GetSystem<T>(forBuild);
		}
		/// <summary>Tries to get an <see cref="EngineSystemBase"/> associated with a registered engine</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="forBuild">The build requesting this system</param>
		/// <returns><see cref="EngineSystemReference{T}.None"/> if the engine doesn't support the requested system</returns>
		public static EngineSystemReference<T> TryGetSystem<T>(EngineBuildHandle forBuild)
			where T : EngineSystemBase
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			BlamEngine engine = Engines[forBuild.EngineIndex];

			return engine.TryGetSystem<T>(forBuild);
		}
		#endregion

		#region Bit encoding
		internal static void BitEncodeResourceModelIndex(ref Bitwise.HandleBitEncoder encoder, int resourceModelIndex)
		{
			if (!IsValidResourceModelIndex(resourceModelIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(resourceModelIndex));
			}

			encoder.EncodeNoneable32(resourceModelIndex, kResourceModelBitMask);
		}
		internal static int BitDecodeResourceModelIndex(uint handle, int bitIndex)
		{
			int index = Bits.BitDecodeNoneable(handle, bitIndex, kResourceModelBitMask);

			if (!IsValidResourceModelIndex(index))
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Decoded resource model index must be NONE or in the range [0, {0}); actual value is {1}.",
					ResourceModels.Count, index));
			}

			return index;
		}
		#endregion
	};
}
