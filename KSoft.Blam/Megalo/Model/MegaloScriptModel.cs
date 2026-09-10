using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int InitializationTriggerIndex {
			get { return mInitializationTriggerIndex; }
			set { mInitializationTriggerIndex = value;
				NotifyPropertyChanged(kInitializationTriggerIndexChangedEventArgs);
		} }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int LocalInitializationTriggerIndex {
			get { return mLocalInitializationTriggerIndex; }
			set { mLocalInitializationTriggerIndex = value;
				NotifyPropertyChanged(kLocalInitializationTriggerIndexChangedEventArgs);
		} }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int HostMigrationTriggerIndex {
			get { return mHostMigrationTriggerIndex; }
			set { mHostMigrationTriggerIndex = value;
				NotifyPropertyChanged(kHostMigrationTriggerIndexChangedEventArgs);
		} }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int DoubleHostMigrationTriggerIndex {
			get { return mDoubleHostMigrationTriggerIndex; }
			set { mDoubleHostMigrationTriggerIndex = value;
				NotifyPropertyChanged(kDoubleHostMigrationTriggerIndexChangedEventArgs);
		} }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int ObjectDeathEventTriggerIndex {
			get { return mObjectDeathEventTriggerIndex; }
			set { mObjectDeathEventTriggerIndex = value;
				NotifyPropertyChanged(kObjectDeathEventTriggerIndexChangedEventArgs);
		} }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int LocalTriggerIndex {
			get { return mLocalTriggerIndex; }
			set { mLocalTriggerIndex = value;
				NotifyPropertyChanged(kLocalTriggerIndexChangedEventArgs);
		} }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int PregameTriggerIndex {
			get { return mPregameTriggerIndex; }
			set { mPregameTriggerIndex = value;
				NotifyPropertyChanged(kPregameTriggerIndexChangedEventArgs);
		} }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int IncidentTriggerIndex {
			get { return mIncidentTriggerIndex; }
			set { mIncidentTriggerIndex = value;
				NotifyPropertyChanged(kIncidentTriggerIndexChangedEventArgs);
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
			{
				return new Games.HaloReach.Megalo.Model.MegaloScriptModelHaloReach(variantManager,
					(variant as Games.HaloReach.RuntimeData.Variants.GameEngineMegaloVariantHaloReach)!);
			}

			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
			{
				return new Games.Halo4.Megalo.Model.MegaloScriptModelHalo4(variantManager,
					(variant as Games.Halo4.RuntimeData.Variants.GameEngineMegaloVariantHalo4)!);
			}

			throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
		}
		#endregion

		#region IndexTargetIsValid
		public bool EnumIndexIsValid(Proto.MegaloScriptValueType enumType, int index)
		{
			if (enumType.BaseType != Proto.MegaloScriptValueBaseType.Enum)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Value type must have base type {0}; actual base type is {1}.",
					Proto.MegaloScriptValueBaseType.Enum, enumType.BaseType), nameof(enumType));
			}

			var e = Database.Enums[enumType.EnumIndex];
			var etraits = enumType.EnumTraits;

			if (etraits == Proto.MegaloScriptValueEnumTraits.HasNoneMember)
			{
				index += 1;
			}

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
			{
				return false;
			}

			var sdb = Database.StaticDatabase;
			return target switch
			{
				Proto.MegaloScriptValueIndexTarget.Undefined =>		true,
				Proto.MegaloScriptValueIndexTarget.Trigger =>		index < Triggers.Count,
				#region Static
				Proto.MegaloScriptValueIndexTarget.ObjectType =>	index < sdb.ObjectTypeList.Types.Count,
				Proto.MegaloScriptValueIndexTarget.Name =>			index < sdb.Names.Count,
				Proto.MegaloScriptValueIndexTarget.Sound =>			index < sdb.Sounds.Count,
				Proto.MegaloScriptValueIndexTarget.Incident =>		index < sdb.Incidents.Count,
				Proto.MegaloScriptValueIndexTarget.HudWidgetIcon =>	index < sdb.HudWidgetIcons.Count,
				Proto.MegaloScriptValueIndexTarget.GameEngineIcon =>index < sdb.GameEngineIcons.Count,
				Proto.MegaloScriptValueIndexTarget.Medal =>			index < sdb.Medals.Count,
				Proto.MegaloScriptValueIndexTarget.Ordnance =>		index < sdb.OrdnanceList.Types.Count,
				#endregion
				#region Variant
				// #TODO_IMPLEMENT: log when we're indexing to an unused loadout palette
				Proto.MegaloScriptValueIndexTarget.LoadoutPalette =>index < MegaloVariant.BaseVariant.LoadoutOptions.Palettes.Length,
				Proto.MegaloScriptValueIndexTarget.Option =>		index < MegaloVariant.UserDefinedOptions.Count,
				Proto.MegaloScriptValueIndexTarget.String =>		index < MegaloVariant.StringTable.Count,
				Proto.MegaloScriptValueIndexTarget.PlayerTraits =>	index < MegaloVariant.PlayerTraits.Count,
				Proto.MegaloScriptValueIndexTarget.Statistic =>		index < GameStatistics.Count,
				Proto.MegaloScriptValueIndexTarget.Widget =>		index < HudWidgets.Count,
				Proto.MegaloScriptValueIndexTarget.ObjectFilter =>	index < ObjectFilters.Count,
				#endregion

				_ => false,
			};
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
			{
				return TypeExtensions.kNone;
			}

#pragma warning disable IDE0059 // Unnecessary assignment of a value
			int id = TypeExtensionsBlam.IndexOfByPropertyNotFoundResult;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			id = target switch
			{
				Proto.MegaloScriptValueIndexTarget.Trigger =>			FindNameIndex(Triggers, name),

				Proto.MegaloScriptValueIndexTarget.Option =>			FindNameIndex(MegaloVariant.UserDefinedOptions, name),
				Proto.MegaloScriptValueIndexTarget.String =>			MegaloVariant.StringTable.FindNameIndex(name),
				Proto.MegaloScriptValueIndexTarget.PlayerTraits =>		FindNameIndex(MegaloVariant.PlayerTraits, name),
				Proto.MegaloScriptValueIndexTarget.Statistic =>			FindNameIndex(GameStatistics, name),
				Proto.MegaloScriptValueIndexTarget.Widget =>			FindNameIndex(HudWidgets, name),
				Proto.MegaloScriptValueIndexTarget.ObjectFilter =>		FindNameIndex(ObjectFilters, name),
				Proto.MegaloScriptValueIndexTarget.GameObjectFilter =>	FindNameIndex(CandySpawnerFilters, name),
				_ => Database.StaticDatabase.FromIndexName(target, name),
			};
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
			{
				return kIndexNameNone;
			}

			return target switch
			{
				Proto.MegaloScriptValueIndexTarget.Trigger =>			Triggers[index].Name,

				Proto.MegaloScriptValueIndexTarget.Option =>			MegaloVariant.UserDefinedOptions[index].CodeName,
				Proto.MegaloScriptValueIndexTarget.String =>			MegaloVariant.StringTable[index].CodeName,
				Proto.MegaloScriptValueIndexTarget.PlayerTraits =>		MegaloVariant.PlayerTraits[index].CodeName,
				Proto.MegaloScriptValueIndexTarget.Statistic =>			GameStatistics[index].CodeName,
				Proto.MegaloScriptValueIndexTarget.Widget =>			HudWidgets[index].CodeName,
				Proto.MegaloScriptValueIndexTarget.ObjectFilter =>		ObjectFilters[index].CodeName,
				Proto.MegaloScriptValueIndexTarget.GameObjectFilter =>	CandySpawnerFilters[index].CodeName,

				_ => Database.StaticDatabase.ToIndexName(target, index),
			};
		}
		internal readonly struct IndexNameResolvingContext
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
			if (!target.HasIndexName())
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Can't get an index value by name for target {0}, which doesn't support naming.",
					target), nameof(target));
			}
			ArgumentException.ThrowIfNullOrEmpty(indexName);

			var id_resolving_ctxt = new IndexNameResolvingContext(this, target);
			int result = IndexNameResolvingContext.IdResolver(id_resolving_ctxt, indexName);
			if (!result.IsNoneOrPositive())
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Couldn't resolve index name '{0}' for target {1}; resolved value was {2}.",
					indexName, target, result), nameof(indexName));
			}

			return result;
		}

		readonly struct TriggerIndexNameResolvingContext
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
					: null!;
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
			{
				s.Stream(ref unionGroupIndex, bit_length);
			}
			else
			{
				s.Write(mCompilerState.UnionGroupRemappings[unionGroupIndex], bit_length);
			}
		}

		protected virtual void SerializeGameObjectFilters(IO.BitStream s)
		{
			int game_object_filter_count = s.IsReading ? 0 : CandySpawnerFilters.Count;
			s.Stream(ref game_object_filter_count, Database.Limits.GameObjectFilters.CountBitLength);
			s.StreamElements(CandySpawnerFilters, Database.Limits.GameObjectFilters.CountBitLength);
			if (CandySpawnerFilters.Count != game_object_filter_count)
			{
				var message = string.Format(Util.InvariantCultureInfo,
					"GameObjectFilters count mismatch; expected {0}, actual {1}.",
					game_object_filter_count, CandySpawnerFilters.Count);
				if (s.IsReading)
				{
					throw new System.IO.InvalidDataException(message);
				}

				throw new InvalidOperationException(message);
			}
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
			{
				SerializeGameObjectFilters(s);
			}
		}
		public void Serialize(IO.BitStream s)
		{
			if (s.IsWriting)
			{
				BeginCompile();
			}

			using (s.EnterOwnerBookmark(this))
			{
				SerializeImpl(s);
			}

			if (s.IsWriting)
			{
				EndCompile();
			}
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
			{
				ObjectTypeReferences[typeIndex] = true;
			}
		}
		internal void ObjectTypeReferenceRemove(int typeIndex)
		{
			if (typeIndex.IsNotNone())
			{
				ObjectTypeReferences[typeIndex] = false;
			}
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
			string? dataTypeName = null)
		{
			ArgumentException.ThrowIfNullOrEmpty(refMemberName);

			MegaloScriptVariableReferenceData.Initialize(this,
				out MegaloScriptVariableReferenceData result,
				refKind,
				out Proto.MegaloScriptProtoVariableReferenceMember member,
				refMemberName, dataTypeName);

			Util.MarkUnusedVariable(ref member);

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
			string? dataTypeName = null)
		{
			ArgumentException.ThrowIfNullOrEmpty(refMemberName);
			ArgumentException.ThrowIfNullOrEmpty(enumMemberName);

			MegaloScriptVariableReferenceData.Initialize(this,
				out MegaloScriptVariableReferenceData result,
				refKind,
				out Proto.MegaloScriptProtoVariableReferenceMember member,
				refMemberName, dataTypeName);

			if (!member.HasDataValue)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Variable reference member '{0}' has no data field, let alone enum data.",
					refMemberName));
			}

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
			string? dataTypeName = null)
		{
			ArgumentException.ThrowIfNullOrEmpty(refMemberName);
			ArgumentNullException.ThrowIfNull(indexName);

			MegaloScriptVariableReferenceData.Initialize(this,
				out MegaloScriptVariableReferenceData result,
				refKind,
				out Proto.MegaloScriptProtoVariableReferenceMember member,
				refMemberName, dataTypeName);

			if (!member.HasDataValue)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Variable reference member '{0}' has no data field, let alone index data.",
					refMemberName));
			}

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
