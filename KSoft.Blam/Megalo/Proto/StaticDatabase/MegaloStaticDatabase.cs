using System.Collections.Generic;

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
		public List<MegaloHudWidgetIcon> HudWidgetIcons { get; private set; }
		public List<GameEngineIcon> GameEngineIcons { get; private set; }
		public List<MegaloEngineSound> Sounds { get; private set; }
		public List<MegaloEngineStringId> Names { get; private set; }

		public bool MultiplayerEffectsAreAvailable => MultiplayerEffects.Count > 0;

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
			HudWidgetIcons = new List<MegaloHudWidgetIcon>(limits.MegaloHudWidgetIcons.MaxCount);
			GameEngineIcons = new List<GameEngineIcon>(limits.GameEngineIcons.MaxCount);
			Sounds = new List<MegaloEngineSound>(limits.MegaloEngineSounds.MaxCount);
			Names = new List<MegaloEngineStringId>(limits.MegaloStringIds.MaxCount);
		}

		#region Index Name resolving
		static int FindNameIndex<T>(IList<T> list, string name)
			where T : IMegaloStaticDataNamedObject
			=> list.IndexOfByProperty(name,
				x => x.Name);
		internal string ToIndexName(MegaloScriptValueIndexTarget target, int index)
		{
			return target switch
			{
				MegaloScriptValueIndexTarget.ObjectType =>		ObjectTypeList.Types[index].Name,
				MegaloScriptValueIndexTarget.Name =>			Names[index].Name,
				MegaloScriptValueIndexTarget.Sound =>			Sounds[index].Name,
				MegaloScriptValueIndexTarget.Incident =>		Incidents[index].Name,
				MegaloScriptValueIndexTarget.HudWidgetIcon =>	HudWidgetIcons[index].Name,
				MegaloScriptValueIndexTarget.GameEngineIcon =>	GameEngineIcons[index].Name,
				MegaloScriptValueIndexTarget.Medal =>			Medals[index].Name,
				MegaloScriptValueIndexTarget.Ordnance =>		OrdnanceList.Sets[index].Name,

				_ => throw new KSoft.Debug.UnreachableException(target.ToString()),
			};
		}
		internal int FromIndexName(MegaloScriptValueIndexTarget target, string name)
		{
#pragma warning disable IDE0059 // Unnecessary assignment of a value
			int id = TypeExtensionsBlam.IndexOfByPropertyNotFoundResult;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

			id = target switch
			{
				MegaloScriptValueIndexTarget.ObjectType =>		FindNameIndex(ObjectTypeList.Types, name),
				MegaloScriptValueIndexTarget.Name =>			FindNameIndex(Names, name),
				MegaloScriptValueIndexTarget.Sound =>			FindNameIndex(Sounds, name),
				MegaloScriptValueIndexTarget.Incident =>		FindNameIndex(Incidents, name),
				MegaloScriptValueIndexTarget.HudWidgetIcon =>	FindNameIndex(HudWidgetIcons, name),
				MegaloScriptValueIndexTarget.GameEngineIcon =>	FindNameIndex(GameEngineIcons, name),
				MegaloScriptValueIndexTarget.Medal =>			FindNameIndex(Medals, name),
				MegaloScriptValueIndexTarget.Ordnance =>		FindNameIndex(OrdnanceList.Sets, name),

				_ => throw new KSoft.Debug.UnreachableException(target.ToString()),
			};
			if (id == TypeExtensionsBlam.IndexOfByPropertyNotFoundResult)
			{
				throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
					"Couldn't find an {0} entry named {1}",
					target, name));
			}

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
		//const string kGroupTagLoadoutGlobalsDefinition = "lgtd";
		const string kGroupTagMegaloGamEngineSounds = "mgls";
		const string kGroupTagMegaloStringIdTable = "msit";

		static void ThrowIfCountExceedsLimit<TDoc, TCursor, T>(IO.TagElementStream<TDoc, TCursor, string> s,
			string listName, ICollection<T> list, int maxCount)
			where TDoc : class
			where TCursor : class
		{
			if (list.Count <= maxCount)
			{
				return;
			}

			string msg = string.Format(Util.InvariantCultureInfo,
				"{0} has {1} entries, maximum is {2}.",
				listName,
				list.Count,
				maxCount);
			if (s.IsReading)
			{
				s.ThrowReadException(new System.IO.InvalidDataException(msg));
			}
			else
			{
				throw new System.InvalidOperationException(msg);
			}
		}

		static void ThrowIfCountDoesNotEqualLimit<TDoc, TCursor, T>(IO.TagElementStream<TDoc, TCursor, string> s,
			string listName, ICollection<T> list, int expectedCount)
			where TDoc : class
			where TCursor : class
		{
			if (list.Count == expectedCount)
			{
				return;
			}

			string msg = string.Format(Util.InvariantCultureInfo,
				"{0} has {1} entries, expected {2}.",
				listName,
				list.Count,
				expectedCount);
			if (s.IsReading)
			{
				s.ThrowReadException(new System.IO.InvalidDataException(msg));
			}
			else
			{
				throw new System.InvalidOperationException(msg);
			}
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			#region MultiplayerEffects (Halo4 only)
			using (var bm = s.EnterCursorBookmarkOpt(kGroupTagMultiplayerEffects, MultiplayerEffects, Predicates.HasItems))
			{
				if (bm.IsNotNull)
				{
					using (	s.EnterCursorBookmark("effects"))
					{
						s.StreamableElements("entry", MultiplayerEffects);
					}
				}
			}
			#endregion

			#region multiplayer_object_type_list
			using (s.EnterCursorBookmark(kGroupTagMultiplayerObjectTypeList))
			{
				ObjectTypeList.Serialize(s);
			}

			ThrowIfCountExceedsLimit(s, nameof(ObjectTypeList), ObjectTypeList.Types,
				mLimits.MultiplayerObjectTypes.MaxCount);
			#endregion

			#region custom_app_globals (Halo4 only)
			using (var bm = s.EnterCursorBookmarkOpt(kGroupTagCustomAppGlobals, CustomApps, Predicates.HasItems))
			{
				if (bm.IsNotNull)
				{
					using (	s.EnterCursorBookmark("apps"))
					{
						s.StreamableElements("entry", CustomApps);
					}
				}
			}
			#endregion

			#region game_globals_ordnance_list (Halo4 only)
			using (var bm = s.EnterCursorBookmarkOpt(kGroupTagGameGlobalsOrdnanceList, OrdnanceList, obj=>!obj.IsAvailable))
			{
				if (bm.IsNotNull)
				{
					OrdnanceList.Serialize(s);

					ThrowIfCountExceedsLimit(s, nameof(OrdnanceList), OrdnanceList.Types,
						mLimits.GameOrdnanceTypes.MaxCount);
				}
			}
			#endregion

			#region game_medal_globals
			using (s.EnterCursorBookmark(kGroupTagGameMedalGlobals))
			using (s.EnterCursorBookmark("medals"))
			{
				s.StreamableElements("entry", Medals);
			}

			ThrowIfCountExceedsLimit(s, nameof(Medals), Medals, mLimits.GameMedals.MaxCount);
			#endregion

			#region incident_globals_definition
			using (s.EnterCursorBookmark(kGroupTagIncidentGlobalsDefinition))
			using (s.EnterCursorBookmark("incidents"))
			{
				s.StreamableElements("entry", Incidents);
			}

			ThrowIfCountExceedsLimit(s, nameof(Incidents), Incidents, mLimits.GameIncidentTypes.MaxCount);
			#endregion

			#region loadout_globals_definition
			#endregion

			#region megalogamengine_sounds
			using (s.EnterCursorBookmark(kGroupTagMegaloGamEngineSounds))
			using (s.EnterCursorBookmark("sounds"))
			{
				s.StreamableElements("entry", Sounds);
			}

			ThrowIfCountDoesNotEqualLimit(s, nameof(Sounds), Sounds, mLimits.MegaloEngineSounds.MaxCount);
			#endregion

			#region megalo_string_id_table
			using (s.EnterCursorBookmark(kGroupTagMegaloStringIdTable))
			using (s.EnterCursorBookmark("names"))
			{
				s.StreamableElements("entry", Names);
			}

			ThrowIfCountExceedsLimit(s, nameof(Names), Names, mLimits.MegaloStringIds.MaxCount);
			#endregion

			#region HudWidgetIcons
			using (s.EnterCursorBookmark("hud_widget_icons"))
			using (s.EnterCursorBookmark("icons"))
			{
				s.StreamableElements("entry", HudWidgetIcons);
			}

			ThrowIfCountExceedsLimit(s, nameof(HudWidgetIcons), HudWidgetIcons,
				mLimits.MegaloHudWidgetIcons.MaxCount);
			#endregion

			#region GameEngineIcons
			using (s.EnterCursorBookmark("engine_icons"))
			using (s.EnterCursorBookmark("icons"))
			{
				s.StreamableElements("entry", GameEngineIcons);
			}

			ThrowIfCountExceedsLimit(s, nameof(GameEngineIcons), GameEngineIcons,
				mLimits.GameEngineIcons.MaxCount);
			#endregion

			if (s.IsReading)
			{
				MultiplayerEffects.TrimExcess();
				CustomApps.TrimExcess();
				Medals.TrimExcess();
				Incidents.TrimExcess();
				Sounds.TrimExcess();
				Names.TrimExcess();
				HudWidgetIcons.TrimExcess();
				GameEngineIcons.TrimExcess();

				for (int x = 0; x < Sounds.Count; x++)
				{
					if (Sounds[x].IsAvailable) { continue; }

					Sounds[x].UpdateForUndefined("MEGALO_SOUND", x);
				}

				for (int x = 0; x < Incidents.Count; x++)
				{
					if (Incidents[x].IsAvailable) { continue; }

					Incidents[x].UpdateForUndefined("INCIDENT", x);
				}

				for (int x = 0; x < HudWidgetIcons.Count; x++)
				{
					if (HudWidgetIcons[x].IsAvailable) { continue; }

					HudWidgetIcons[x].UpdateForUndefined("HUD_WIDGET", x);
				}

				for (int x = 0; x < GameEngineIcons.Count; x++)
				{
					if (GameEngineIcons[x].IsAvailable) { continue; }

					GameEngineIcons[x].UpdateForUndefined("ENGINE_ICON", x);
				}
			}
		}
		#endregion
	};
}
