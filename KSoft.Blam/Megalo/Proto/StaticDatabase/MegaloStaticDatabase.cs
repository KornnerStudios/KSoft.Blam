using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloStaticDatabase
		: IO.ITagElementStringNameStreamable
	{
		public Engine.EngineBuildHandle EngineBuild { get; private set; }

		readonly EngineLimits mLimits;

		public List<MultiplayerEffect> MultiplayerEffects { get; private set; }
		public MultiplayerObjectTypeList ObjectTypeList { get; private set; }
		public List<UnitCustomApp> CustomApps { get; private set; }
		public GameGlobalsOrdnanceList OrdnanceList { get; private set; }
		public List<GameMedal> Medals { get; private set; }
		public List<GameIncident> Incidents { get; private set; }
		public List<MegaloEngineSound> Sounds { get; private set; }
		public List<MegaloEngineStringId> Names { get; private set; }

		public bool MultiplayerEffectsAreAvailable { get { return MultiplayerEffects.Count > 0; } }

		internal MegaloStaticDatabase(Engine.EngineBuildHandle forBuild, EngineLimits limits)
		{
			EngineBuild = forBuild;

			mLimits = limits;

			MultiplayerEffects = new List<MultiplayerEffect>();
			ObjectTypeList = new MultiplayerObjectTypeList(limits);
			CustomApps = new List<UnitCustomApp>();
			OrdnanceList = new GameGlobalsOrdnanceList(limits);
			Medals = new List<GameMedal>(limits.GameMedals.MaxCount);
			Incidents = new List<GameIncident>(limits.GameIncidentTypes.MaxCount);
			Sounds = new List<MegaloEngineSound>(limits.MegaloEngineSounds.MaxCount);
			Names = new List<MegaloEngineStringId>(limits.MegaloStringIds.MaxCount);
		}

		#region Index Name resolving
		static int FindNameIndex<T>(IList<T> list, string name)
			where T : IMegaloStaticDataNamedObject
		{
			return list.IndexOfByProperty(name,
				x => x.Name);
		}
		internal string ToIndexName(MegaloScriptValueIndexTarget target, int index)
		{
			switch (target)
			{
				case MegaloScriptValueIndexTarget.ObjectType:	return ObjectTypeList.Types[index].Name;
				case MegaloScriptValueIndexTarget.Name:			return Names[index].Name;
				case MegaloScriptValueIndexTarget.Sound:		return Sounds[index].Name;
				case MegaloScriptValueIndexTarget.Incident:		return Incidents[index].Name;
				case MegaloScriptValueIndexTarget.Medal:		return Medals[index].Name;
				case MegaloScriptValueIndexTarget.Ordnance:		return OrdnanceList.Sets[index].Name;

				default: throw new KSoft.Debug.UnreachableException(target.ToString());
			}
		}
		internal int FromIndexName(MegaloScriptValueIndexTarget target, string name)
		{
			int id = TypeExtensionsBlam.IndexOfByPropertyNotFoundResult;

			switch (target)
			{
				case MegaloScriptValueIndexTarget.ObjectType:	id = FindNameIndex(ObjectTypeList.Types, name); break;
				case MegaloScriptValueIndexTarget.Name:			id = FindNameIndex(Names, name); break;
				case MegaloScriptValueIndexTarget.Sound:		id = FindNameIndex(Sounds, name); break;
				case MegaloScriptValueIndexTarget.Incident:		id = FindNameIndex(Incidents, name); break;
				case MegaloScriptValueIndexTarget.Medal:		id = FindNameIndex(Medals, name); break;
				case MegaloScriptValueIndexTarget.Ordnance:		id = FindNameIndex(OrdnanceList.Sets, name); break;

				default: throw new KSoft.Debug.UnreachableException(target.ToString());
			}

			if (id == TypeExtensionsBlam.IndexOfByPropertyNotFoundResult)
				throw new KeyNotFoundException(string.Format("Couldn't find an {0} entry named {1}",
					target, name));

			return id;
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		const string kGroupTagMultiplayerEffects = "mgee";
		const string kGroupTagMultiplayerObjectTypeList = "motl";
		const string kGroupTagCustomAppGlobals = "capg";
		const string kGroupTagGameGlobalsOrdnanceList = "ggol";
		const string kGroupTagGameMedalGlobals = "gmeg";
		const string kGroupTagIncidentGlobalsDefinition = "ingd";
		const string kGroupTagLoadoutGlobalsDefinition = "lgtd";
		const string kGroupTagMegaloGamEngineSounds = "mgls";
		const string kGroupTagMegaloStringIdTable = "msit";

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			#region MultiplayerEffects (Halo4 only)
			using (var bm = s.EnterCursorBookmarkOpt(kGroupTagMultiplayerEffects, MultiplayerEffects, Predicates.HasItems)) if (bm.IsNotNull)
			using (			s.EnterCursorBookmark("effects"))
				s.StreamableElements("entry", MultiplayerEffects);
			#endregion

			#region multiplayer_object_type_list
			using (s.EnterCursorBookmark(kGroupTagMultiplayerObjectTypeList))
				ObjectTypeList.Serialize(s);

			Contract.Assert(ObjectTypeList.Types.Count <= mLimits.MultiplayerObjectTypes.MaxCount);
			#endregion

			#region custom_app_globals (Halo4 only)
			using (var bm = s.EnterCursorBookmarkOpt(kGroupTagCustomAppGlobals, CustomApps, Predicates.HasItems)) if (bm.IsNotNull)
			using (			s.EnterCursorBookmark("apps"))
				s.StreamableElements("entry", CustomApps);
			#endregion

			#region game_globals_ordnance_list (Halo4 only)
			using (var bm = s.EnterCursorBookmarkOpt(kGroupTagGameGlobalsOrdnanceList, OrdnanceList, obj=>!obj.IsAvailable)) if (bm.IsNotNull) {
				OrdnanceList.Serialize(s);

				Contract.Assert(OrdnanceList.Types.Count <= mLimits.GameOrdnanceTypes.MaxCount);
			}
			#endregion

			#region game_medal_globals
			using (s.EnterCursorBookmark(kGroupTagGameMedalGlobals))
			using (s.EnterCursorBookmark("medals"))
				s.StreamableElements("entry", Medals);

			Contract.Assert(Medals.Count <= mLimits.GameMedals.MaxCount);
			#endregion

			#region incident_globals_definition
			using (s.EnterCursorBookmark(kGroupTagIncidentGlobalsDefinition))
			using (s.EnterCursorBookmark("incidents"))
				s.StreamableElements("entry", Incidents);

			Contract.Assert(Incidents.Count <= mLimits.GameIncidentTypes.MaxCount);
			#endregion

			#region loadout_globals_definition
			#endregion

			#region megalogamengine_sounds
			using (s.EnterCursorBookmark(kGroupTagMegaloGamEngineSounds))
			using (s.EnterCursorBookmark("sounds"))
				s.StreamableElements("entry", Sounds);

			Contract.Assert(Sounds.Count == mLimits.MegaloEngineSounds.MaxCount);
			#endregion

			#region megalo_string_id_table
			using (s.EnterCursorBookmark(kGroupTagMegaloStringIdTable))
			using (s.EnterCursorBookmark("names"))
				s.StreamableElements("entry", Names);

			Contract.Assert(Names.Count <= mLimits.MegaloStringIds.MaxCount);
			#endregion

			if (s.IsReading)
			{
				MultiplayerEffects.TrimExcess();
				CustomApps.TrimExcess();
				Medals.TrimExcess();
				Incidents.TrimExcess();
				Sounds.TrimExcess();
				Names.TrimExcess();

				for (int x = 0; x < Sounds.Count; x++)
				{
					if (Sounds[x].IsAvailable) continue;

					Sounds[x].UpdateForUndefined("MEGALO_SOUND", x);
				}

				for (int x = 0; x < Incidents.Count; x++)
				{
					if (Incidents[x].IsAvailable) continue;

					Incidents[x].UpdateForUndefined("INCIDENT", x);
				}
			}
		}
		#endregion
	};
}