using System;
using System.Collections.Generic;

namespace KSoft.Blam.Engine
{
	/// <summary>The system root for a registered engine in <see cref="EngineRegistry"/></summary>
	public sealed partial class BlamEngine
		: IO.ITagElementStringNameStreamable
	{
		#region Constants
		internal const int kMaxCount = 8 - 1; // 3 bits. per registry
		internal static readonly int kIndexBitCount;
		private static readonly uint kIndexBitMask =
			Bits.GetNoneableEncodingTraits(kMaxCount,
				out kIndexBitCount);
		#endregion

		/// <summary>A handle with just the engine index populated, that tracks back to this engine instance</summary>
		public EngineBuildHandle RootBuildHandle { get; private set; }

		public string Name { get; private set; } = string.Empty;

		/// <summary>The community-determined generation this engine was introduced</summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Local - written to via reflection in SerializePrototype
		public EngineGeneration Generation { get; private set; }

		/// <summary>The data store which has all the know builds based on this general engine</summary>
		public EngineBuildRepository BuildRepository { get; private set; } = new();

		readonly Dictionary<Values.KGuid, BlamEngineSystem> mSystemPrototypes = new();

		public override string ToString() => Name;

		#region Engine System interfaces
		Dictionary<Values.KGuid, EngineSystemBase> mActiveSystems;

		public bool SupportsSystem(Values.KGuid systemGuid)
		{
			if (systemGuid == Values.KGuid.Empty)
			{
				throw new ArgumentException("System GUID must not be empty.", nameof(systemGuid));
			}

			return mSystemPrototypes.TryGetValue(systemGuid, out BlamEngineSystem /*proto_system*/_);
		}

		/// <summary>Only call me if you are <see cref="EngineSystemBase.RemoveReferenceAsync"/></summary>
		/// <param name="activeSystem">The system which no longer has any active references</param>
		internal void CloseSystem(EngineSystemBase activeSystem)
		{
			ArgumentNullException.ThrowIfNull(activeSystem);

			var system_guid = activeSystem.Prototype.SystemMetadata.SystemGuid;
			lock (mActiveSystems)
			{
				if (!mActiveSystems.Remove(system_guid))
				{
					throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"System {0} was not active for engine {1}.",
						system_guid.ToString(Values.KGuid.kFormatHyphenated, Util.InvariantCultureInfo), Name));
				}
			}
		}

		EngineSystemBase GetNewOrExistingSystem(EngineSystemAttribute systemMetadata)
		{
			// All we care about doing here is getting or constructing a new system.
			// Referencing counting and the like is handled elsewhere (hopefully only in EngineSystemReference)

			var proto_system = mSystemPrototypes[systemMetadata.SystemGuid];

			EngineSystemBase system;
			lock (mActiveSystems)
			{
				if (!mActiveSystems.TryGetValue(systemMetadata.SystemGuid, out system))
				{
					system = systemMetadata.NewInstance(proto_system);
					mActiveSystems.Add(systemMetadata.SystemGuid, system);
				}
			}

			return system;
		}
		EngineSystemBase GetSystem(Values.KGuid systemGuid, EngineBuildHandle forBuild)
		{
			if (!SupportsSystem(systemGuid))
			{
				string system_display_name = EngineRegistry.GetSystemDebugDisplayString(systemGuid);

				string msg = string.Format(Util.InvariantCultureInfo,
					"{0} doesn't support the system {1}",
					forBuild.ToDisplayString(), system_display_name);

				throw new InvalidOperationException(msg);
			}

			var system_metadata = EngineRegistry.TryGetRegisteredSystem(systemGuid);
			if (system_metadata == null)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"System {0} is not registered.",
					systemGuid.ToString(Values.KGuid.kFormatHyphenated, Util.InvariantCultureInfo)));
			}

			return GetNewOrExistingSystem(system_metadata);
		}
		EngineSystemBase TryGetSystem(Values.KGuid systemGuid)
		{
			if (!SupportsSystem(systemGuid))
			{
				return null;
			}

			EngineSystemAttribute system_metadata = EngineRegistry.TryGetRegisteredSystem(systemGuid);
			if (system_metadata == null)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"System {0} is not registered.",
					systemGuid.ToString(Values.KGuid.kFormatHyphenated, Util.InvariantCultureInfo)));
			}

			return GetNewOrExistingSystem(system_metadata);
		}

		public EngineSystemReference<T> GetSystem<T>(EngineBuildHandle forBuild)
			where T : EngineSystemBase
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}
			if (forBuild.EngineIndex != RootBuildHandle.EngineIndex)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Build handle engine index must be {0}; actual value is {1}.",
					RootBuildHandle.EngineIndex, forBuild.EngineIndex), nameof(forBuild));
			}

			Values.KGuid system_guid = EngineSystemAttribute.GetSystemGuid<T>();
			var system = (T)GetSystem(system_guid, forBuild);

			return new EngineSystemReference<T>(system, forBuild);
		}
		public EngineSystemReference<T> GetSystem<T>()
			where T : EngineSystemBase
		{
			return GetSystem<T>(RootBuildHandle);
		}

		public EngineSystemReference<T> TryGetSystem<T>(EngineBuildHandle forBuild)
			where T : EngineSystemBase
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}
			if (forBuild.EngineIndex != RootBuildHandle.EngineIndex)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Build handle engine index must be {0}; actual value is {1}.",
					RootBuildHandle.EngineIndex, forBuild.EngineIndex), nameof(forBuild));
			}

			Values.KGuid system_guid = EngineSystemAttribute.GetSystemGuid<T>();
			var system = (T)TryGetSystem(system_guid);

			return system == null
				? EngineSystemReference<T>.None
				: new EngineSystemReference<T>(system, forBuild);
		}
		public EngineSystemReference<T> TryGetSystem<T>()
			where T : EngineSystemBase
		{
			return TryGetSystem<T>(RootBuildHandle);
		}
		#endregion

		#region Bit encoding
		internal static void BitEncodeIndex(ref Bitwise.HandleBitEncoder encoder, int engineIndex)
		{
			if (!engineIndex.IsNoneOrPositive() || engineIndex >= EngineRegistry.Engines.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(engineIndex));
			}

			encoder.EncodeNoneable32(engineIndex, kIndexBitMask);
		}
		internal static int BitDecodeIndex(uint handle, int bitIndex)
		{
			int index = Bits.BitDecodeNoneable(handle, bitIndex, kIndexBitMask);

			if (!IsValidIndex(index))
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Decoded engine index must be NONE or in the range [0, {0}); actual value is {1}.",
					EngineRegistry.Engines.Count, index));
			}

			return index;
		}
		#endregion

		#region Index/Id interfaces
		/// <summary>Initialize the <see cref="RootBuildHandle"/> for all <see cref="EngineRegistry.Engines"/></summary>
		internal static void InitializeEngineBuildHandles()
		{
			foreach (BlamEngine engine in EngineRegistry.Engines)
			{
				engine.RootBuildHandle = EngineBuildHandle.Create(EngineIdResolver(null, engine.Name));
			}
		}
		/// <summary>Initialize the <see cref="BuildRepository"/> handles all <see cref="EngineRegistry.Engines"/></summary>
		internal static void InitializeEngineRepositoryBuildHandles()
		{
			foreach (BlamEngine engine in EngineRegistry.Engines)
			{
				engine.BuildRepository.InitializeBuildHandles();
			}
		}
		internal static EngineBuildBranch ResolveWellKnownEngineBranch(string engineName, string branchName)
		{
			ArgumentException.ThrowIfNullOrEmpty(engineName);
			ArgumentException.ThrowIfNullOrEmpty(branchName);

			int engine_index = EngineIdResolverSansKeyNotFoundException(null, engineName);

			return engine_index.IsNone()
				? null
				: EngineRegistry.Engines[engine_index].BuildRepository.ResolveWellKnownEngineBranch(branchName);
		}

		public static bool IsValidIndex(int engineIndex) =>
			engineIndex.IsNoneOrPositive() && engineIndex < EngineRegistry.Engines.Count;

		static int EngineIdResolver(object _null, string name)
		{
			int id = TypeExtensions.kNone;

			if (!string.IsNullOrEmpty(name))
			{
				id = EngineRegistry.Engines.FindIndex(x => x.Name == name);

				if (id.IsNone())
				{
					throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
						"No engine is registered with the name '{0}'",
						name));
				}
			}

			return id;
		}
		static readonly Func<object, string, int> EngineIdResolverSansKeyNotFoundException =
			(_null, name) => !string.IsNullOrEmpty(name)
				? EngineRegistry.Engines.FindIndex(x => x.Name == name)
				: TypeExtensions.kNone;
		static readonly Func<object, int, string> EngineNameResolver =
			(_null, id) => id.IsNotNone()
				? EngineRegistry.Engines[id].Name
				: null;
		#endregion
	};
}
