using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Variants = RuntimeData.Variants;

	[System.Reflection.Obfuscation(Exclude = false)]
	public abstract partial class MegaloScriptModel
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		const uint kVersion = 1;

		public Proto.MegaloScriptDatabase Database { get; private set; }
		public Variants.GameEngineMegaloVariant MegaloVariant { get; private set; }

		public Collections.ActiveList<MegaloScriptUnionGroup> UnionGroups { get; private set; }
		public Collections.ActiveList<MegaloScriptValueBase> Values { get; private set; }
		public Collections.ActiveList<MegaloScriptVirtualTrigger> VirtualTriggers { get; private set; }
		public ObservableCollection<int> TriggerExecutionOrder { get; private set; }

		public Collections.ActiveList<MegaloScriptCondition> Conditions { get; private set; }
		public Collections.ActiveList<MegaloScriptAction> Actions { get; private set; }
		public Collections.ActiveList<MegaloScriptTrigger> Triggers { get; private set; }
		public ObservableCollection<MegaloScriptGameStatistic> GameStatistics { get; private set; }
		#region Variable Sets
		public MegaloScriptModelVariableSet GlobalVariables { get; private set; }
		public MegaloScriptModelVariableSet PlayerVariables { get; private set; }
		public MegaloScriptModelVariableSet ObjectVariables { get; private set; }
		public MegaloScriptModelVariableSet TeamVariables { get; private set; }
		#endregion
		public ObservableCollection<MegaloScriptHudWidget> HudWidgets { get; private set; }
		#region Trigger entry points
		int mInitializationTriggerIndex;
		int mLocalInitializationTriggerIndex;
		int mHostMigrationTriggerIndex;
		int mDoubleHostMigrationTriggerIndex;
		int mObjectDeathEventTriggerIndex;
		int mLocalTriggerIndex;
		int mPregameTriggerIndex;
		int mIncidentTriggerIndex;
		#endregion
		protected Collections.BitSet ObjectTypeReferences { get; private set; }
		public ObservableCollection<MegaloScriptObjectFilter> ObjectFilters { get; private set; }
		// Halo4
		public ObservableCollection<MegaloScriptGameObjectFilter> CandySpawnerFilters { get; private set; }

		#region Trigger entry points properties
		public int InitializationTriggerIndex {
			get { return mInitializationTriggerIndex; }
			set { mInitializationTriggerIndex = value;
				NotifyPropertyChanged(kInitializationTriggerIndexChanged);
		} }
		public int LocalInitializationTriggerIndex {
			get { return mLocalInitializationTriggerIndex; }
			set { mLocalInitializationTriggerIndex = value;
				NotifyPropertyChanged(kLocalInitializationTriggerIndexChanged);
		} }
		public int HostMigrationTriggerIndex {
			get { return mHostMigrationTriggerIndex; }
			set { mHostMigrationTriggerIndex = value;
				NotifyPropertyChanged(kHostMigrationTriggerIndexChanged);
		} }
		public int DoubleHostMigrationTriggerIndex {
			get { return mDoubleHostMigrationTriggerIndex; }
			set { mDoubleHostMigrationTriggerIndex = value;
				NotifyPropertyChanged(kDoubleHostMigrationTriggerIndexChanged);
		} }
		public int ObjectDeathEventTriggerIndex {
			get { return mObjectDeathEventTriggerIndex; }
			set { mObjectDeathEventTriggerIndex = value;
				NotifyPropertyChanged(kObjectDeathEventTriggerIndexChanged);
		} }
		public int LocalTriggerIndex {
			get { return mLocalTriggerIndex; }
			set { mLocalTriggerIndex = value;
				NotifyPropertyChanged(kLocalTriggerIndexChanged);
		} }
		public int PregameTriggerIndex {
			get { return mPregameTriggerIndex; }
			set { mPregameTriggerIndex = value;
				NotifyPropertyChanged(kPregameTriggerIndexChanged);
		} }
		public int IncidentTriggerIndex {
			get { return mIncidentTriggerIndex; }
			set { mIncidentTriggerIndex = value;
				NotifyPropertyChanged(kIncidentTriggerIndexChanged);
		} }
		#endregion

		protected MegaloScriptModel(Variants.GameEngineVariant variantManager, Variants.GameEngineMegaloVariant variant)
		{
			Database = variantManager.MegaloProtoSystem.GetMegaloDatabaseAsync(variantManager.GameBuild).Result;
			MegaloVariant = variant;

			var db = Database;

			UnionGroups = new Collections.ActiveList<MegaloScriptUnionGroup>(db.Limits.UnionGroupsDesc);
			Values = new Collections.ActiveList<MegaloScriptValueBase>(db.Limits.ScriptValuesDesc);
			VirtualTriggers = new Collections.ActiveList<MegaloScriptVirtualTrigger>(db.Limits.VirtualTriggersDesc);
			TriggerExecutionOrder = new ObservableCollection<int>();

			Conditions = new Collections.ActiveList<MegaloScriptCondition>(db.Limits.ConditionsDesc);
			Actions = new Collections.ActiveList<MegaloScriptAction>(db.Limits.ActionsDesc);
			Triggers = new Collections.ActiveList<MegaloScriptTrigger>(db.Limits.TriggersDesc);
			GameStatistics = new ObservableCollection<MegaloScriptGameStatistic>();
			#region Variable Sets
			GlobalVariables = new MegaloScriptModelVariableSet(Database, MegaloScriptVariableSet.Globals);
			PlayerVariables = new MegaloScriptModelVariableSet(Database, MegaloScriptVariableSet.Player);
			ObjectVariables = new MegaloScriptModelVariableSet(Database, MegaloScriptVariableSet.Object);
			TeamVariables = new MegaloScriptModelVariableSet(Database, MegaloScriptVariableSet.Team);
			#endregion
			HudWidgets = new ObservableCollection<MegaloScriptHudWidget>();
			#region Trigger entry points
			InitializationTriggerIndex = LocalInitializationTriggerIndex =
				HostMigrationTriggerIndex = DoubleHostMigrationTriggerIndex =
				ObjectDeathEventTriggerIndex =
				LocalTriggerIndex = PregameTriggerIndex =
				IncidentTriggerIndex =
					TypeExtensions.kNone;
			#endregion
			ObjectTypeReferences = new Collections.BitSet(db.Limits.MultiplayerObjectTypes.MaxCount);
			ObjectFilters = new ObservableCollection<MegaloScriptObjectFilter>();
			CandySpawnerFilters = new ObservableCollection<MegaloScriptGameObjectFilter>();
		}

		#region Type Factories
		internal static MegaloScriptModel Create(Variants.GameEngineVariant variantManager, Variants.GameEngineMegaloVariant variant)
		{
			var gameBuild = variantManager.GameBuild;

			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.HaloReach.Megalo.Model.MegaloScriptModelHaloReach(variantManager,
					variant as Games.HaloReach.RuntimeData.Variants.GameEngineMegaloVariantHaloReach);

			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
				return new Games.Halo4.Megalo.Model.MegaloScriptModelHalo4(variantManager,
					variant as Games.Halo4.RuntimeData.Variants.GameEngineMegaloVariantHalo4);

			throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
		}
		#endregion

		#region IndexTargetIsValid
		public bool EnumIndexIsValid(Proto.MegaloScriptValueType enumType, int index)
		{
			Contract.Requires(enumType.BaseType == Proto.MegaloScriptValueBaseType.Enum);

			var e = Database.Enums[enumType.EnumIndex];
			var etraits = enumType.EnumTraits;

			if (etraits == Proto.MegaloScriptValueEnumTraits.HasNoneMember)
				index += 1;

			return index >= 0 && index < e.Members.Count;
		}

		/// <summary>Validates that the index is valid based on its target and traits</summary>
		/// <param name="target">Target which the index is for</param>
		/// <param name="traits">Specify <see cref="Proto.MegaloScriptValueIndexTraits.Reference"/> if <paramref name="index"/> can't be NONE</param>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool IndexTargetIsValid(Proto.MegaloScriptValueIndexTarget target, Proto.MegaloScriptValueIndexTraits traits,
			int index)
		{
			if (traits == Proto.MegaloScriptValueIndexTraits.Reference && index < 0)
				return false;

			var sdb = Database.StaticDatabase;
			switch (target)
			{
				case Proto.MegaloScriptValueIndexTarget.Undefined:		return true;
				case Proto.MegaloScriptValueIndexTarget.Trigger:		return index < Triggers.Count;
				#region Static
				case Proto.MegaloScriptValueIndexTarget.ObjectType:		return index < sdb.ObjectTypeList.Types.Count;
				case Proto.MegaloScriptValueIndexTarget.Name:			return index < sdb.Names.Count;
				case Proto.MegaloScriptValueIndexTarget.Sound:			return index < sdb.Sounds.Count;
				case Proto.MegaloScriptValueIndexTarget.Incident:		return index < sdb.Incidents.Count;
				case Proto.MegaloScriptValueIndexTarget.HudWidgetIcon:	return index < sdb.HudWidgetIcons.Count;
				case Proto.MegaloScriptValueIndexTarget.GameEngineIcon:	return index < sdb.GameEngineIcons.Count;
				case Proto.MegaloScriptValueIndexTarget.Medal:			return index < sdb.Medals.Count;
				case Proto.MegaloScriptValueIndexTarget.Ordnance:		return index < sdb.OrdnanceList.Types.Count;
				#endregion
				#region Variant
				// #TODO_IMPLEMENT: log when we're indexing to an unused loadout palette
				case Proto.MegaloScriptValueIndexTarget.LoadoutPalette: return index < MegaloVariant.BaseVariant.LoadoutOptions.Palettes.Length;
				case Proto.MegaloScriptValueIndexTarget.Option:			return index < MegaloVariant.UserDefinedOptions.Count;
				case Proto.MegaloScriptValueIndexTarget.String:			return index < MegaloVariant.StringTable.Count;
				case Proto.MegaloScriptValueIndexTarget.PlayerTraits:	return index < MegaloVariant.PlayerTraits.Count;
				case Proto.MegaloScriptValueIndexTarget.Statistic:		return index < GameStatistics.Count;
				case Proto.MegaloScriptValueIndexTarget.Widget:			return index < HudWidgets.Count;
				case Proto.MegaloScriptValueIndexTarget.ObjectFilter:	return index < ObjectFilters.Count;
				#endregion
			}

			return false;
		}
		#endregion

		#region Index Name resolving
		internal const string kIndexNameNone = "NONE";

		internal static int FindNameIndex<T>(IList<T> list, string name)
			where T : IMegaloScriptAccessibleObject
		{
			return list.IndexOfByProperty(name,
				x => x.CodeName);
		}
		internal int FromIndexName(Proto.MegaloScriptValueIndexTarget target, string name)
		{
			if (name == kIndexNameNone)
				return TypeExtensions.kNone;

			int id = TypeExtensionsBlam.IndexOfByPropertyNotFoundResult;
			switch (target)
			{
				case Proto.MegaloScriptValueIndexTarget.Trigger:			id = FindNameIndex(Triggers, name); break;

				case Proto.MegaloScriptValueIndexTarget.Option:				id = FindNameIndex(MegaloVariant.UserDefinedOptions, name); break;
				case Proto.MegaloScriptValueIndexTarget.String:				id = MegaloVariant.StringTable.FindNameIndex(name);  break;
				case Proto.MegaloScriptValueIndexTarget.PlayerTraits:		id = FindNameIndex(MegaloVariant.PlayerTraits, name); break;
				case Proto.MegaloScriptValueIndexTarget.Statistic:			id = FindNameIndex(GameStatistics, name); break;
				case Proto.MegaloScriptValueIndexTarget.Widget:				id = FindNameIndex(HudWidgets, name); break;
				case Proto.MegaloScriptValueIndexTarget.ObjectFilter:		id = FindNameIndex(ObjectFilters, name);  break;
				case Proto.MegaloScriptValueIndexTarget.GameObjectFilter:	id = FindNameIndex(CandySpawnerFilters, name); break;

				default: id = Database.StaticDatabase.FromIndexName(target, name); break;
			}

			if (id == TypeExtensionsBlam.IndexOfByPropertyNotFoundResult)
			{
				throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
					"Couldn't find an {0} object named {1}",
					target, name));
			}

			return id;
		}
		internal string ToIndexName(Proto.MegaloScriptValueIndexTarget target, int index)
		{
			if (index.IsNone())
				return kIndexNameNone;

			switch (target)
			{
				case Proto.MegaloScriptValueIndexTarget.Trigger:			return Triggers[index].Name;

				case Proto.MegaloScriptValueIndexTarget.Option:				return MegaloVariant.UserDefinedOptions[index].CodeName;
				case Proto.MegaloScriptValueIndexTarget.String:				return MegaloVariant.StringTable[index].CodeName;
				case Proto.MegaloScriptValueIndexTarget.PlayerTraits:		return MegaloVariant.PlayerTraits[index].CodeName;
				case Proto.MegaloScriptValueIndexTarget.Statistic:			return GameStatistics[index].CodeName;
				case Proto.MegaloScriptValueIndexTarget.Widget:				return HudWidgets[index].CodeName;
				case Proto.MegaloScriptValueIndexTarget.ObjectFilter:		return ObjectFilters[index].CodeName;
				case Proto.MegaloScriptValueIndexTarget.GameObjectFilter:	return CandySpawnerFilters[index].CodeName;

				default: return Database.StaticDatabase.ToIndexName(target, index);
			}
		}
		internal struct IndexNameResolvingContext
		{
			readonly MegaloScriptModel Model;
			readonly Proto.MegaloScriptValueIndexTarget IndexTarget;

			public IndexNameResolvingContext(MegaloScriptModel model, Proto.MegaloScriptValueIndexTarget indexTarget)
			{ Model = model; IndexTarget = indexTarget; }

			public static readonly Func<IndexNameResolvingContext, string, int> IdResolver =
				(ctxt, name) => ctxt.Model.FromIndexName(ctxt.IndexTarget, name);
			public static readonly Func<IndexNameResolvingContext, int, string> NameResolver =
				(ctxt, id) => ctxt.Model.ToIndexName(ctxt.IndexTarget, id);
		};

		internal int GetTargetIndexFromName(Proto.MegaloScriptValueIndexTarget target, string indexName)
		{
			Contract.Requires(target.HasIndexName(), "Can't get an index value by name which doesn't support naming");
			Contract.Requires(!string.IsNullOrEmpty(indexName));

			var id_resolving_ctxt = new IndexNameResolvingContext(this, target);
			int result = IndexNameResolvingContext.IdResolver(id_resolving_ctxt, indexName);
			if (!result.IsNoneOrPositive())
				throw new ArgumentException(indexName);

			return result;
		}

		struct TriggerIndexNameResolvingContext
		{
			readonly MegaloScriptModel Model;

			public TriggerIndexNameResolvingContext(MegaloScriptModel model)
			{ Model = model; }

			public static readonly Func<TriggerIndexNameResolvingContext, string, int> IdResolver =
				(ctxt, name) =>
					!string.IsNullOrEmpty(name)
						? ctxt.Model.FromIndexName(Proto.MegaloScriptValueIndexTarget.Trigger, name)
						: TypeExtensions.kNone;

			public static readonly Func<TriggerIndexNameResolvingContext, int, string> NameResolver =
				(ctxt, id) =>
					id.IsNotNone()
					? ctxt.Model.ToIndexName(Proto.MegaloScriptValueIndexTarget.Trigger, id)
					: null;
		};
		#endregion

		#region IBitStreamSerializable Members
		/// <remarks>
		/// When reading, <paramref name="unionGroupIndex"/> will be local to the callee
		///
		/// When writing, <paramref name="unionGroupIndex"/> should be an active item in <see cref="UnionGroups"/>
		/// </remarks>
		internal void StreamLocalUnionGroupIndex(IO.BitStream s, ref int unionGroupIndex)
		{
			int bit_length = Database.Limits.Conditions.IndexBitLength;

			if (s.IsReading)
				s.Stream(ref unionGroupIndex, bit_length);
			else
				s.Write(mCompilerState.UnionGroupRemappings[unionGroupIndex], bit_length);
		}

		protected virtual void SerializeGameObjectFilters(IO.BitStream s)
		{
			int game_object_filter_count = s.IsReading ? 0 : CandySpawnerFilters.Count;
			s.Stream(ref game_object_filter_count, Database.Limits.GameObjectFilters.CountBitLength);
			s.StreamElements(CandySpawnerFilters, Database.Limits.GameObjectFilters.CountBitLength);
			Contract.Assert(CandySpawnerFilters.Count == game_object_filter_count);
		}
		protected virtual void SerializeImpl(IO.BitStream s)
		{
			var condition_write_order = s.IsWriting ? mCompilerState.ConditionWriteOrder : null;
			var action_write_order = s.IsWriting ? mCompilerState.ActionWriteOrder : null;
			var trigger_write_order = s.IsWriting ? mCompilerState.TriggerWriteOrder : null;

			Collections.ActiveListUtil.Serialize(s, Conditions, Database.Limits.Conditions.CountBitLength,
				this, NewConditionFromBitStream, condition_write_order);
			Collections.ActiveListUtil.Serialize(s, Actions, Database.Limits.Actions.CountBitLength,
				this, NewActionFromBitStream, action_write_order);
			Collections.ActiveListUtil.Serialize(s, Triggers, Database.Limits.Triggers.CountBitLength,
				this, NewTriggerFromBitStream, trigger_write_order);
			s.StreamElements(GameStatistics, Database.Limits.GameStatistics.CountBitLength, this, _model => _model.NewGameStatistic());
			GlobalVariables.Serialize(this, s);
			PlayerVariables.Serialize(this, s);
			ObjectVariables.Serialize(this, s);
			TeamVariables.Serialize(this, s);
			s.StreamElements(HudWidgets, Database.Limits.HudWidgets.CountBitLength);
			SerializeTriggerEntryPoints(s);
			ObjectTypeReferences.SerializeWords(s, Shell.EndianFormat.Little);
			s.StreamElements(ObjectFilters, Database.Limits.ObjectFilters.CountBitLength);
			if (Database.Limits.SupportsGameObjectFilters)
				SerializeGameObjectFilters(s);
		}
		public void Serialize(IO.BitStream s)
		{
			if (s.IsWriting)
				BeginCompile();

			using (s.EnterOwnerBookmark(this))
				SerializeImpl(s);

			if (s.IsWriting)
				EndCompile();
			else
			{
				BeginDecompile();
				EndDecompile();
			}
		}
		#endregion


		#region ObjectTypeReferences interfaces
		internal void ObjectTypeReferencesClear()
		{
			ObjectTypeReferences.Clear();
		}

		internal void ObjectTypeReferenceAdd(int typeIndex)
		{
			if (typeIndex.IsNotNone())
				ObjectTypeReferences[typeIndex] = true;
		}
		internal void ObjectTypeReferenceRemove(int typeIndex)
		{
			if (typeIndex.IsNotNone())
				ObjectTypeReferences[typeIndex] = false;
		}

		public int ObjectTypeReferenceAdd(string typeName)
		{
			int index = FromIndexName(Proto.MegaloScriptValueIndexTarget.ObjectType, typeName);

			ObjectTypeReferenceAdd(index);

			return index;
		}
		public void ObjectTypeReferenceRemove(string typeName)
		{
			int index = FromIndexName(Proto.MegaloScriptValueIndexTarget.ObjectType, typeName);

			ObjectTypeReferenceRemove(index);
		}
		#endregion

		#region NewVarReference
		/// <summary></summary>
		/// <param name="refKind">The kind of variable we're this reference</param>
		/// <param name="refMemberName">The variable's reference type</param>
		/// <param name="dataValue">Actual 'data' value for the variable reference</param>
		/// <param name="dataTypeName">Optional type info for <paramref name="dataValue"/></param>
		/// <returns></returns>
		public MegaloScriptVariableReferenceData NewVarReference(MegaloScriptVariableReferenceType refKind,
			string refMemberName, int dataValue = TypeExtensions.kNone,
			string dataTypeName = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(refMemberName));

			MegaloScriptVariableReferenceData result;
			Proto.MegaloScriptProtoVariableReferenceMember member;
			MegaloScriptVariableReferenceData.Initialize(this,
				out result, refKind, out member, refMemberName, dataTypeName);

			result.Data = dataValue;

			return result;
		}
		/// <summary></summary>
		/// <param name="refKind">The kind of variable we're this reference</param>
		/// <param name="refMemberName">The variable's reference type</param>
		/// <param name="enumMemberName">Actual 'data' value for the variable reference</param>
		/// <param name="dataTypeName">Optional type info for <paramref name="enumMemberName"/></param>
		/// <returns></returns>
		public MegaloScriptVariableReferenceData NewVarReferenceWithEnumData(MegaloScriptVariableReferenceType refKind,
			string refMemberName, string enumMemberName,
			string dataTypeName = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(refMemberName));
			Contract.Requires(!string.IsNullOrEmpty(enumMemberName));

			MegaloScriptVariableReferenceData result;
			Proto.MegaloScriptProtoVariableReferenceMember member;
			MegaloScriptVariableReferenceData.Initialize(this,
				out result, refKind, out member, refMemberName, dataTypeName);

			Contract.Assert(member.HasDataValue, "Member has no data field, let alone enum data");

			var id_resolving_ctxt = new Proto.MegaloScriptEnum.EnumNameResolvingContext(Database, member.EnumValueType);
			result.Data = Proto.MegaloScriptEnum.EnumNameResolvingContext.IdResolver(id_resolving_ctxt, enumMemberName);

			return result;
		}
		/// <summary></summary>
		/// <param name="refKind">The kind of variable we're this reference</param>
		/// <param name="refMemberName">The variable's reference type</param>
		/// <param name="indexName">Actual 'data' value for the variable reference</param>
		/// <param name="dataTypeName">Optional type info for <paramref name="indexName"/></param>
		/// <returns></returns>
		public MegaloScriptVariableReferenceData NewVarReferenceWithIndexData(MegaloScriptVariableReferenceType refKind,
			string refMemberName, string indexName,
			string dataTypeName = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(refMemberName));
			Contract.Requires(indexName != null);

			MegaloScriptVariableReferenceData result;
			Proto.MegaloScriptProtoVariableReferenceMember member;
			MegaloScriptVariableReferenceData.Initialize(this,
				out result, refKind, out member, refMemberName, dataTypeName);

			Contract.Assert(member.HasDataValue, "Member has no data field, let alone index data");

			result.Data = GetTargetIndexFromName(member.ValueType.IndexTarget, indexName);

			return result;
		}
		#endregion

		public MegaloScriptTrigger CreateTrigger(string codeName,
			MegaloScriptTriggerExecutionMode mode = MegaloScriptTriggerExecutionMode.General,
			MegaloScriptTriggerType type = MegaloScriptTriggerType.Normal)
		{
			var trigger = CreateTrigger();
			trigger.Name = codeName ?? "Trigger" + trigger.Id;
			trigger.ExecutionMode = mode;
			trigger.TriggerType = type;

			return trigger;
		}

		public KeyValuePair<int, MegaloScriptObjectFilter> CreateObjectFilter(string codeName,
			int labelStringIndex)
		{
			var filter = new MegaloScriptObjectFilter {
				CodeName = codeName,
				LabelStringIndex = labelStringIndex
			};

			ObjectFilters.Add(filter);
			return new KeyValuePair<int, MegaloScriptObjectFilter>(ObjectFilters.Count-1, filter);
		}
	};
}
