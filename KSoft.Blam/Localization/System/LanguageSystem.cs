#define LANGUAGE_SYSTEM_USE_ONLY_ONE_TABLE

using System;
using System.Collections.Generic;
using System.Linq;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Localization
{
	/// <summary>Interface for language related services for an engine and its builds</summary>
	[Engine.EngineSystem(KeepExternsLoaded=true)]
	public sealed class LanguageSystem
		: Engine.EngineSystemBase
	{
		#region SystemGuid
		static readonly Values.KGuid kSystemGuid = new Values.KGuid("EF39D343-DAD5-43D4-A215-F91722ED1CC5");
		public static Values.KGuid SystemGuid { get { return kSystemGuid; } }
		#endregion

#if LANGUAGE_SYSTEM_USE_ONLY_ONE_TABLE
		// As it stands, all engines only need one table. All of their branches and pre-ship builds don't use different lang sets
		readonly GameLanguageTable mEngineTable;
#else
		readonly Dictionary<Engine.EngineBuildHandle, GameLanguageTable> mEngineTables;
#endif

		internal LanguageSystem()
		{
#if LANGUAGE_SYSTEM_USE_ONLY_ONE_TABLE
			mEngineTable = new GameLanguageTable();
#else
			mEngineTables = new Dictionary<Engine.EngineBuildHandle, GameLanguageTable>();
#endif
		}

		public GameLanguageTable GetLanguageTable(Engine.EngineBuildHandle forBuild)
		{
#if LANGUAGE_SYSTEM_USE_ONLY_ONE_TABLE
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);
			Contract.Assert(forBuild.EngineIndex == mEngineTable.BuildHandle.EngineIndex);

			return mEngineTable;
#else
			GameLanguageTable engine_table = null;
			forBuild.TryGetValue(mEngineTables, ref engine_table);

			return engine_table;
#endif
		}

		#region ITagElementStreamable<string> Members
		protected override void SerializeExternBody<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			const string kElementNameLanguageTable = "T";

			using (s.EnterCursorBookmark("LanguageTables"))
			{
#if LANGUAGE_SYSTEM_USE_ONLY_ONE_TABLE
				using (s.EnterCursorBookmark(kElementNameLanguageTable))
				{
					mEngineTable.Serialize(s);
				}

				if (s.IsReading)
				{
					Contract.Assert(s.ElementsByName(kElementNameLanguageTable).Count() == 1,
						"Engine has multiple tables defined! This is unexpected, backend code needs to be rewritten");
				}
#else
				s.StreamableElements(kElementNameLanguageTable,
					mEngineTables, this.Engine.RootBuildHandle,
					Blam.Engine.EngineBuildHandle.SerializeWithBaseline);
#endif
			}
		}
		#endregion

		internal static GameLanguageTable GetGameLanguageTable(Engine.EngineBuildHandle forBuild)
		{
			Contract.Requires(!forBuild.IsNone);

			using (var system_ref = Blam.Engine.EngineRegistry.GetSystem<LanguageSystem>(forBuild))
			{
				var system = system_ref.System;
				return system.GetLanguageTable(forBuild);
			}
		}
	};
}