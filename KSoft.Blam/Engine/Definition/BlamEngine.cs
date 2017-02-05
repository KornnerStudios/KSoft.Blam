using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Engine
{
	/// <summary>The system root for a registered engine in <see cref="EngineRegistry"/></summary>
	public sealed class BlamEngine
		: IO.ITagElementStringNameStreamable
	{
		#region Constants
		internal const int kMaxCount = 8 - 1; // 3 bits. per registry
		internal static readonly int kIndexBitCount;
		private static readonly uint kIndexBitMask;

		static BlamEngine()
		{
			kIndexBitMask = Bits.GetNoneableEncodingTraits(kMaxCount,
				out kIndexBitCount);
		}
		#endregion
		
		/// <summary>A handle with just the engine index populated, that tracks back to this engine instance</summary>
		public EngineBuildHandle RootBuildHandle { get; private set; }

		public string Name { get; private set; }
		
		/// <summary>The community-determined generation this engine was introduced</summary>
		public EngineGeneration Generation { get; private set; }
		
		/// <summary>The data store which has all the know builds based on this general engine</summary>
		public EngineBuildRepository BuildRepository { get; private set; }

		Dictionary<Values.KGuid, string> mDeclaredSystems;

		public BlamEngine()
		{
			Name = 
				"";

			BuildRepository = new EngineBuildRepository();
			mDeclaredSystems = new Dictionary<Values.KGuid, string>();
		}

		public override string ToString()
		{
			return Name;
		}

		#region Engine System interfaces
		Dictionary<Values.KGuid, EngineSystemBase> mActiveSystems;

		public bool SupportsSystem(Values.KGuid systemGuid)
		{
			Contract.Requires<ArgumentNullException>(systemGuid != Values.KGuid.Empty);

			string system_file;
			return mDeclaredSystems.TryGetValue(systemGuid, out system_file);
		}

		EngineSystemBase GetNewOrExistingSystem(EngineSystemAttribute systemMetadata)
		{
			EngineSystemBase system;
			if (!mActiveSystems.TryGetValue(systemMetadata.Guid, out system))
			{
				system = systemMetadata.NewInstance();
				mActiveSystems.Add(systemMetadata.Guid, system);
			}

			return system;
		}
		EngineSystemBase GetSystem(Values.KGuid systemGuid, EngineBuildHandle forBuild)
		{
			if (!SupportsSystem(systemGuid))
			{
				string msg = string.Format("{0} doesn't support the system {1}",
					forBuild.ToDisplayString(), systemGuid.ToString(Values.KGuid.kFormatHyphenated));

				throw new InvalidOperationException(msg);
			}

			var system_metadata = EngineRegistry.TryGetRegisteredSystem(systemGuid);
			Contract.Assume(system_metadata != null);

			return GetNewOrExistingSystem(system_metadata);
		}
		EngineSystemBase TryGetSystem(Values.KGuid systemGuid)
		{
			if (!SupportsSystem(systemGuid))
				return null;

			var system_metadata = EngineRegistry.TryGetRegisteredSystem(systemGuid);
			Contract.Assume(system_metadata != null);

			return GetNewOrExistingSystem(system_metadata);
		}

		public EngineSystemReference<T> GetSystem<T>(EngineBuildHandle forBuild)
			where T : EngineSystemBase
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);
			Contract.Requires(forBuild.EngineIndex == RootBuildHandle.EngineIndex);

			var system_guid = EngineSystemAttribute.GetSystemGuid<T>();
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
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);
			Contract.Requires(forBuild.EngineIndex == RootBuildHandle.EngineIndex);

			var system_guid = EngineSystemAttribute.GetSystemGuid<T>();
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

		#region ITagElementStreamable<string> Members
		static void StreamDeclaredSystemsKey<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			BlamEngine engine,
			ref Values.KGuid systemGuid)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("guid", ref systemGuid);

			if (s.IsReading)
			{
				string invalid_guid_msg = null;

				if (systemGuid == Values.KGuid.Empty)
					invalid_guid_msg = "Invalid system guid";
				else if (EngineRegistry.TryGetRegisteredSystem(systemGuid) == null)
					invalid_guid_msg = "Unknown system guid";

				if (invalid_guid_msg != null)
					s.ThrowReadException(new System.IO.InvalidDataException(invalid_guid_msg));
			}
		}
		static void StreamDeclaredSystemsValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			BlamEngine engine,
			ref string externsFile)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("externs", ref externsFile, Predicates.IsNotNullOrEmpty);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			Contract.Assert(s.Owner == null);
			s.Owner = this;

			using (s.EnterUserDataBookmark(this))
			{
				BuildRepository.Serialize(s);
			}

			using (var bm = s.EnterCursorBookmarkOpt("Systems", mDeclaredSystems, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamElements("System", mDeclaredSystems, this,
					StreamDeclaredSystemsKey, StreamDeclaredSystemsValue,
					_ctxt => null);

			if (s.IsReading)
			{
				if (mDeclaredSystems.Count > 0)
					mActiveSystems = new Dictionary<Values.KGuid, EngineSystemBase>(mDeclaredSystems.Count);
			}
		}

		internal static void SerializePrototype<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			object _context, ref BlamEngine engine)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("generation", engine, obj => obj.Generation);
			s.StreamAttribute("name", engine, obj => obj.Name);
		}
		#endregion

		#region Bit encoding
		internal static void BitEncodeIndex(ref Bitwise.HandleBitEncoder encoder, int engineIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(
				engineIndex.IsNoneOrPositive() && engineIndex < EngineRegistry.Engines.Count);

			encoder.EncodeNoneable32(engineIndex, kIndexBitMask);
		}
		internal static int BitDecodeIndex(uint handle, int bitIndex)
		{
			var index = Bits.BitDecodeNoneable(handle, bitIndex, kIndexBitMask);

			Contract.Assert(index.IsNoneOrPositive() && index < EngineRegistry.Engines.Count);
			return index;
		}
		#endregion

		#region Index/Id interfaces
		/// <summary>Initialize the <see cref="RootBuildHandle"/> for all <see cref="EngineRegistry.Engines"/></summary>
		internal static void InitializeEngineBuildHandles()
		{
			foreach (var engine in EngineRegistry.Engines)
				engine.RootBuildHandle = EngineBuildHandle.Create(EngineIdResolver(null, engine.Name));
		}
		/// <summary>Initialize the <see cref="BuildRepository"/> handles all <see cref="EngineRegistry.Engines"/></summary>
		internal static void InitializeEngineRepositoryBuildHandles()
		{
			foreach (var engine in EngineRegistry.Engines)
				engine.BuildRepository.InitializeBuildHandles();
		}
		internal static EngineBuildBranch ResolveWellKnownEngineBranch(string engineName, string branchName)
		{
			Contract.Requires(!string.IsNullOrEmpty(engineName));
			Contract.Requires(!string.IsNullOrEmpty(branchName));

			int engine_index = EngineIdResolverSansKeyNotFoundException(null, engineName);

			return engine_index.IsNone()
				? null
				: EngineRegistry.Engines[engine_index].BuildRepository.ResolveWellKnownEngineBranch(branchName);
		}

		[Contracts.Pure]
		public static bool IsValidIndex(int engineIndex)
		{
			return engineIndex.IsNoneOrPositive() && engineIndex < EngineRegistry.Engines.Count;
		}

		static int EngineIdResolver(object _null, string name)
		{
			int id = TypeExtensions.kNone;

			if (!string.IsNullOrEmpty(name))
			{
				id = EngineRegistry.Engines.FindIndex(x => x.Name == name);

				if (id.IsNone())
					throw new KeyNotFoundException(string.Format("No engine is registered with the name '{0}'",
						name));
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

		internal static bool SerializeId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int engineId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref engineId, null,
					EngineIdResolver, EngineNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
					engineId = TypeExtensions.kNone;
			}
			else
			{
				s.StreamAttributeIdAsString(attributeName, ref engineId, null,
					EngineIdResolver, EngineNameResolver);
			}

			return streamed;
		}
		#endregion
	};
}