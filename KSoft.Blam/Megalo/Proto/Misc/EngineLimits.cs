
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class EngineLimits
		: IO.ITagElementStringNameStreamable
	{
		#region Internal
//		internal Collections.ActiveListDesc<Editor.LogicalGroupUI> TriggerGroupsDesc { get; private set; }
		internal Collections.ActiveListDesc<Model.MegaloScriptVirtualTrigger> VirtualTriggersDesc { get; private set; }
		internal Collections.ActiveListDesc<Model.MegaloScriptValueBase> ScriptValuesDesc { get; private set; }
		#endregion

		#region Scripting
		public MegaloScriptTriggerEntryPoints SupportedEntryPoints;
		public bool SupportsVirtualTriggers;
		/// <summary>Hack for Reach, Reach, Reach</summary>
		internal bool StreamEntryPointIndexAsPointer;
		/// <summary>Hack for Reach, Reach, Reach</summary>
		internal bool StreamConditionsAndActionsRefsAsRefs;

		internal Collections.ActiveListDesc<Model.MegaloScriptUnionGroup> UnionGroupsDesc { get; private set; }

		public ListLimitTraits Conditions = ListLimitTraits.Null;
		internal Collections.ActiveListDesc<Model.MegaloScriptCondition> ConditionsDesc { get; private set; }

		public ListLimitTraits Actions = ListLimitTraits.Null;
		internal Collections.ActiveListDesc<Model.MegaloScriptAction> ActionsDesc { get; private set; }

		public ListLimitTraits Triggers = ListLimitTraits.Null;
		internal Collections.ActiveListDesc<Model.MegaloScriptTrigger> TriggersDesc { get; private set; }

		public bool Supports(MegaloScriptTriggerEntryPoints entryPoints)
		{
			return (SupportedEntryPoints & entryPoints) == entryPoints;
		}
		#endregion

		#region Static
		public ListLimitTraits GameEngineOptions = ListLimitTraits.Null;
		public ListLimitTraits MultiplayerObjectTypes = ListLimitTraits.Null;
		public ListLimitTraits MegaloStringIds = ListLimitTraits.Null;
		public ListLimitTraits MegaloEngineSounds = ListLimitTraits.Null;
		public ListLimitTraits GameIncidentTypes = ListLimitTraits.Null;
		public ListLimitTraits GameMedals = ListLimitTraits.Null;
		public ListLimitTraits GameOrdnanceTypes = ListLimitTraits.Null;

		public ListLimitTraits EngineCategories = ListLimitTraits.Null;
		#endregion

		#region Variant
		public ListLimitTraits LoadoutPalettes = ListLimitTraits.Null;

		public ListLimitTraits UserDefinedOptions = ListLimitTraits.Null;
		public ListLimitTraits UserDefinedOptionValues = ListLimitTraits.Null;
		public ListLimitTraits VariantStrings = ListLimitTraits.Null;
		public ListLimitTraits Loadouts = ListLimitTraits.Null;
		public ListLimitTraits PlayerTraits = ListLimitTraits.Null;

		public ListLimitTraits GameStatistics = ListLimitTraits.Null;
		public ListLimitTraits HudWidgets = ListLimitTraits.Null;
		public ListLimitTraits ObjectFilters = ListLimitTraits.Null;
		public ListLimitTraits GameObjectFilters = ListLimitTraits.Null;

		public bool SupportsGameObjectFilters { get { return GameObjectFilters.MaxCount > 0; } }
		#endregion

		internal int GetIndexTargetBitLength(MegaloScriptValueIndexTarget target, MegaloScriptValueIndexTraits traits)
		{
			// use the index bit length (true) or the count bit length (false)
			bool use_index = traits != MegaloScriptValueIndexTraits.Pointer;
			switch (target)
			{
				#region Scripting
				case MegaloScriptValueIndexTarget.Trigger:			return Triggers.GetBitLength(use_index);
				#endregion
				#region Static
				case MegaloScriptValueIndexTarget.ObjectType:		return MultiplayerObjectTypes.IndexBitLength;
				case MegaloScriptValueIndexTarget.Name:				return MegaloStringIds.IndexBitLength;
				case MegaloScriptValueIndexTarget.Sound:			return MegaloEngineSounds.IndexBitLength;
				case MegaloScriptValueIndexTarget.Incident:			return GameIncidentTypes.IndexBitLength;
				//case MegaloScriptValueIndexTarget.Icon:				return 0;
				case MegaloScriptValueIndexTarget.Medal:			return GameMedals.IndexBitLength;
				case MegaloScriptValueIndexTarget.Ordnance:			return GameOrdnanceTypes.IndexBitLength;
				#endregion
				#region Variant
				case MegaloScriptValueIndexTarget.LoadoutPalette:	return LoadoutPalettes.GetBitLength(use_index);
				case MegaloScriptValueIndexTarget.Option:			return UserDefinedOptions.GetBitLength(use_index);
				case MegaloScriptValueIndexTarget.String:			return VariantStrings.GetBitLength(use_index);
				case MegaloScriptValueIndexTarget.PlayerTraits:		return PlayerTraits.GetBitLength(use_index);
				case MegaloScriptValueIndexTarget.Statistic:		return GameStatistics.GetBitLength(use_index);
				case MegaloScriptValueIndexTarget.Widget:			return HudWidgets.GetBitLength(use_index);
				case MegaloScriptValueIndexTarget.ObjectFilter:		return ObjectFilters.GetBitLength(use_index);
				#endregion

				default: throw new KSoft.Debug.UnreachableException(target.ToString());
			}
		}
		#region ITagElementStringNameStreamable Members
		const string kMaxTriggerGroupsElementName = "MaxTriggerGroups";
		const string kMaxVirtualTriggersElementName = "MaxVirtualTriggers";
		const string kMaxScriptValuesElementName = "MaxScriptValues";

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			#region Internal
			if (reading)
			{
				int max_trigger_groups = 0, max_virtual_triggers = 0, max_script_values = 0
					;

				s.ReadElement(kMaxTriggerGroupsElementName, ref max_trigger_groups);
				s.ReadElementOpt(kMaxVirtualTriggersElementName, ref max_virtual_triggers);
				s.ReadElement(kMaxScriptValuesElementName, ref max_script_values);

#if false // #TODO_BLAM
				TriggerGroupsDesc = Collections.ActiveListDesc<Editor.LogicalGroupUI>.
					CreateForNullData(max_trigger_groups, fixedLength: false);
#endif
				VirtualTriggersDesc = Collections.ActiveListDesc<Model.MegaloScriptVirtualTrigger>.
					CreateForNullData(max_virtual_triggers, fixedLength: false);
				VirtualTriggersDesc.ObjectToIndex = Model.MegaloScriptModelObject.kObjectToIndex;

				ScriptValuesDesc = Collections.ActiveListDesc<Model.MegaloScriptValueBase>.
					CreateForNullData(max_script_values, fixedLength: false);
				ScriptValuesDesc.ObjectToIndex = Model.MegaloScriptModelObject.kObjectToIndex;
			}
			else
			{
//				s.WriteElement(kMaxTriggerGroupsElementName, TriggerGroupsDesc.Capacity);
				s.WriteElement(kMaxVirtualTriggersElementName, VirtualTriggersDesc.Capacity);
				s.WriteElement(kMaxScriptValuesElementName, ScriptValuesDesc.Capacity);
			}
			#endregion

			#region Scripting
			s.StreamElementEnum("EntryPoints", ref SupportedEntryPoints, true);
			s.StreamElementOpt("SupportsVirtualTriggers", ref SupportsVirtualTriggers);
			s.StreamElementOpt("StreamEntryPointIndexAsPointer", ref StreamEntryPointIndexAsPointer);
			s.StreamElementOpt("StreamConditionsAndActionsRefsAsRefs", ref StreamConditionsAndActionsRefsAsRefs);

			ListLimitTraits.SerializeViaElement(s, "MaxConditions", ref Conditions);
			ListLimitTraits.SerializeViaElement(s, "MaxActions", ref Actions);
			ListLimitTraits.SerializeViaElement(s, "MaxTriggers", ref Triggers);

			if (reading)
			{
				UnionGroupsDesc = Collections.ActiveListDesc<Model.MegaloScriptUnionGroup>.
					CreateForNullData(Conditions.MaxCount);
				UnionGroupsDesc.ObjectToIndex = Model.MegaloScriptModelObject.kObjectToIndex;

				ConditionsDesc = Collections.ActiveListDesc<Model.MegaloScriptCondition>.
					CreateForNullData(Conditions.MaxCount);
				ConditionsDesc.ObjectToIndex = Model.MegaloScriptModelObject.kObjectToIndex;

				ActionsDesc = Collections.ActiveListDesc<Model.MegaloScriptAction>.
					CreateForNullData(Actions.MaxCount);
				ActionsDesc.ObjectToIndex = Model.MegaloScriptModelObject.kObjectToIndex;

				TriggersDesc = Collections.ActiveListDesc<Model.MegaloScriptTrigger>.
					CreateForNullData(Triggers.MaxCount);
				TriggersDesc.ObjectToIndex = Model.MegaloScriptModelObject.kObjectToIndex;
			}
			#endregion

			#region Static
			ListLimitTraits.SerializeViaElement(s, "NumberOfGameEngineOptions", ref GameEngineOptions);
			ListLimitTraits.SerializeViaElement(s, "MaxMultiplayerObjectTypes", ref MultiplayerObjectTypes);
			ListLimitTraits.SerializeViaElement(s, "MaxMegaloStringIds", ref MegaloStringIds);
			ListLimitTraits.SerializeViaElement(s, "MaxMegaloEngineSounds", ref MegaloEngineSounds);
			ListLimitTraits.SerializeViaElement(s, "MaxGameIncidentTypes", ref GameIncidentTypes);
			ListLimitTraits.SerializeViaElement(s, "MaxGameMedals", ref GameMedals);
			ListLimitTraits.SerializeViaElementOpt(s, "MaxGameOrdnanceTypes", ref GameOrdnanceTypes);

			ListLimitTraits.SerializeViaElement(s, "NumberOfEngineCategories", ref EngineCategories);
			#endregion

			#region Variant
			ListLimitTraits.SerializeViaElement(s, "NumberOfLoadoutPalettes", ref LoadoutPalettes);
			ListLimitTraits.SerializeViaElement(s, "MaxUserDefinedOptions", ref UserDefinedOptions);
			ListLimitTraits.SerializeViaElement(s, "MaxUserDefinedOptionValues", ref UserDefinedOptionValues);
			ListLimitTraits.SerializeViaElement(s, "MaxVariantStrings", ref VariantStrings);
			ListLimitTraits.SerializeViaElementOpt(s, "MaxLoadouts", ref Loadouts);
			ListLimitTraits.SerializeViaElement(s, "MaxPlayerTraits", ref PlayerTraits);
			ListLimitTraits.SerializeViaElement(s, "MaxGameStatistics", ref GameStatistics);
			ListLimitTraits.SerializeViaElement(s, "MaxHudWidgets", ref HudWidgets);
			ListLimitTraits.SerializeViaElement(s, "MaxObjectFilters", ref ObjectFilters);

			ListLimitTraits.SerializeViaElementOpt(s, "MaxGameObjectFilters", ref GameObjectFilters);
			#endregion
		}
		#endregion
	};
}