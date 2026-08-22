#define LANGUAGE_SYSTEM_USE_ONLY_ONE_TABLE

using System;
using System.Collections.Generic;
using System.Linq;

namespace KSoft.Blam.Localization
{
	/// <summary>Interface for language related services for an engine and its builds</summary>
	[Engine.EngineSystem(KeepExternsLoaded=true)]
	public sealed class LanguageSystem
		: Engine.EngineSystemBase
	{
		public static Values.KGuid SystemGuid { get; } = new Values.KGuid("EF39D343-DAD5-43D4-A215-F91722ED1CC5");

#if LANGUAGE_SYSTEM_USE_ONLY_ONE_TABLE
		// As it stands, all engines only need one table. All of their branches and pre-ship builds don't use
		// different lang sets
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
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}
			if (forBuild.EngineIndex != mEngineTable.BuildHandle.EngineIndex)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Build handle engine index must be {0}; actual value is {1}.",
					mEngineTable.BuildHandle.EngineIndex, forBuild.EngineIndex), nameof(forBuild));
			}

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
					int table_count = s.ElementsByName(kElementNameLanguageTable).Count();
					if (table_count != 1)
					{
						s.ThrowReadException(new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
							"Expected exactly one engine language table, but found {0}.", table_count)));
					}
				}
#else
				s.StreamableElements(kElementNameLanguageTable,
					mEngineTables, this.RootBuildHandle,
					Blam.Engine.EngineBuildHandle.SerializeWithBaseline);
#endif
			}
		}
		#endregion

		internal static GameLanguageTable GetGameLanguageTable(Engine.EngineBuildHandle forBuild)
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			using (var system_ref = Blam.Engine.EngineRegistry.GetSystem<LanguageSystem>(forBuild))
			{
				var system = system_ref.System;
				return system.GetLanguageTable(forBuild);
			}
		}
	};
}