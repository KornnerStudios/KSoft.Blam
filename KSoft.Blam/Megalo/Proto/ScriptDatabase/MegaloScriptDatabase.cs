using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TextWriter = System.IO.TextWriter;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptDatabase
		: IO.ITagElementStringNameStreamable
	{
		// old, temp setup used these globals
		internal static MegaloScriptDatabase HaloReach, Halo4;

		public Engine.EngineBuildHandle EngineBuild { get; private set; }

		public uint Version;

		public EngineLimits Limits { get; private set; }
		public MegaloStaticDatabase StaticDatabase { get; private set; }

		public List<SingleEncoding> SingleEncodings { get; private set; }
		public List<MegaloScriptEnum> Enums { get; private set; }
		public List<string> ValueTypeNames { get; private set; }
		public List<MegaloScriptValueType> ValueTypes { get; private set; }

		public List<MegaloScriptProtoCondition> Conditions { get; private set; }
		public List<MegaloScriptProtoActionTemplate> ActionTemplates { get; private set; }
		public List<MegaloScriptProtoAction> Actions { get; private set; }

		public Dictionary<MegaloScriptVariableReferenceType, MegaloScriptProtoVariableReference> VariableRefTypes { get; private set; }
		public Dictionary<MegaloScriptVariableSet, MegaloScriptProtoVariableSet> VariableSets { get; private set; }

		Dictionary<string, MegaloScriptValueType> NameToValueType { get; /*private*/ set; }
		Dictionary<string, MegaloScriptProtoCondition> NameToConditionMap { get; /*private*/ set; }
		Dictionary<string, MegaloScriptProtoAction> NameToActionMap { get; /*private*/ set; }
		// #TODO_BLAM: create a dictionary of bitsets for MegaloScriptValueIndexTarget for conditions/actions which
		// can (possibly) reference non-static data

		internal MegaloScriptValueType TeamDesignatorValueType;
		internal MegaloScriptValueType ObjectTypeIndexValueType;

		internal MegaloScriptProtoAction ActivateTriggerAction;
		internal MegaloScriptProtoAction ActivateVirtualTriggerAction;

		#region ObjectReferenceWithPlayerVarIndex
		internal struct ProtoObjectReferenceWithPlayerVarIndex
		{
			// PlayerReferenceType
			const string kObject_PlayerVar_Name = "Object.PlayerVar";
			const string kPlayer_PlayerVar_Name = "Player.PlayerVar";
			// ObjectReferenceType
			const string kPlayer_SlaveObject_Name = "Player.SlaveObject";

			MegaloScriptValueType PlayerIndexObject, PlayerIndexPlayer;
			MegaloScriptProtoVariableReference ObjectReferenceType;
			int FirstSlaveObjectTypeIndex;
			int HighestSlaveObjectTypeIndex;

			public void Initialize(MegaloScriptDatabase db)
			{
				var player_ref_type = db.VariableRefTypes[MegaloScriptVariableReferenceType.Player];
				PlayerIndexObject = player_ref_type.NameToMember[kObject_PlayerVar_Name].ValueType;
				PlayerIndexPlayer = player_ref_type.NameToMember[kPlayer_PlayerVar_Name].ValueType;

				ObjectReferenceType = db.VariableRefTypes[MegaloScriptVariableReferenceType.Object];
				// #NOTE_BLAM: assumes Player.SlaveObject is the first reference type and everything after it are player variable SlaveObject references
				FirstSlaveObjectTypeIndex =
					ObjectReferenceType.NameToMember[kPlayer_SlaveObject_Name].TypeIndex;
				HighestSlaveObjectTypeIndex = ObjectReferenceType.Members.Count - 1;
			}

			#region Stream PlayerVarIndex interfaces
			bool TypeIndexRefersToPlayerSlaveObject(int refTypeIndex)
			{
				return refTypeIndex >= FirstSlaveObjectTypeIndex && refTypeIndex <= HighestSlaveObjectTypeIndex;
			}
			MegaloScriptValueType GetPlayerVarIndexValueType(int refTypeIndex)
			{
				return !TypeIndexRefersToPlayerSlaveObject(refTypeIndex) ? PlayerIndexObject : PlayerIndexPlayer;
			}
			bool ValidatePlayerVarIndex(int playerVarIndex,
				int refTypeIndex, Model.MegaloScriptModel model)
			{
				if (playerVarIndex.IsNone())
					return true;

				var varIndexType = GetPlayerVarIndexValueType(refTypeIndex);
				return model.VarIndexIsValid(varIndexType.VarType, varIndexType.VarSet, playerVarIndex);
			}
			public void StreamPlayerVarIndex(IO.BitStream s, ref int playerVarIndex,
				int refTypeIndex, Model.MegaloScriptModel model)
			{
				Contract.Assert(refTypeIndex >= 0 && refTypeIndex < ObjectReferenceType.Members.Count);

				var varIndexType = GetPlayerVarIndexValueType(refTypeIndex);
				s.StreamIndex(ref playerVarIndex, varIndexType.BitLength);
			}
			public void StreamPlayerVarIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref int playerVarIndex,
				int refTypeIndex, Model.MegaloScriptModel model)
				where TDoc : class
				where TCursor : class
			{
				const string kAttributeNamePlayerVarIndex = "playerVarIndex";

				if ((model.TagElementStreamSerializeFlags & Model.MegaloScriptModelTagElementStreamFlags.UseIndexNames) != 0)
				{
					var varIndexType = GetPlayerVarIndexValueType(refTypeIndex);

					var resolving_ctxt = new Model.MegaloScriptModelVariableSet.IndexNameResolvingContext(model, varIndexType.VarSet, varIndexType.VarType,
						supportNone: true);
					s.StreamAttributeIdAsString(kAttributeNamePlayerVarIndex, ref playerVarIndex, resolving_ctxt,
						Model.MegaloScriptModelVariableSet.IndexNameResolvingContext.IdResolver,
						Model.MegaloScriptModelVariableSet.IndexNameResolvingContext.NameResolver);
				}
				else
					s.StreamAttribute(kAttributeNamePlayerVarIndex, ref playerVarIndex);

				Contract.Assert(ValidatePlayerVarIndex(playerVarIndex, refTypeIndex, model));
			}
			#endregion
		};
		internal ProtoObjectReferenceWithPlayerVarIndex ObjectReferenceWithPlayerVarIndex;
		#endregion

		#region Ctor
		public MegaloScriptDatabase(Engine.EngineBuildHandle forBuild)
		{
			EngineBuild = forBuild;

			Limits = new EngineLimits();

			SingleEncodings = new List<SingleEncoding>();
			Enums = new List<MegaloScriptEnum>();
			ValueTypeNames = new List<string>();
			ValueTypes = new List<MegaloScriptValueType>();

			Conditions = new List<MegaloScriptProtoCondition>();
			ActionTemplates = new List<MegaloScriptProtoActionTemplate>();
			Actions = new List<MegaloScriptProtoAction>();

			VariableRefTypes = new Dictionary<MegaloScriptVariableReferenceType, MegaloScriptProtoVariableReference>();
			VariableSets = new Dictionary<MegaloScriptVariableSet, MegaloScriptProtoVariableSet>();

			NameToValueType = new Dictionary<string, MegaloScriptValueType>();
			NameToConditionMap = new Dictionary<string, MegaloScriptProtoCondition>();
			NameToActionMap = new Dictionary<string, MegaloScriptProtoAction>();

			ImportCodeEnum<MegaloScriptComparisonType>			("ComparisonType");
			ImportCodeEnum<MegaloScriptOperationType>			("OperationType");
			ImportCodeEnum<RuntimeData.GameTeamDesignator>		("TeamDesignator");
			ImportCodeEnum<GameEngineTimerRate>					("TimerRate");
			ImportCodeEnum<MegaloScriptNavpointIconType>		("NavpointIconType");
			ImportCodeEnum<MegaloScriptDamageReportingModifier>	("DamageReportingModifier");

			if (forBuild.IsChildOf(Engine.EngineRegistry.EngineBranchHaloReach.BranchHandle))
				InitializeForHaloReach();
			else if (forBuild.IsChildOf(Engine.EngineRegistry.EngineBranchHalo4.BranchHandle))
				InitializeForHalo4();
			else
			{
				Contract.Assert(false);
				throw new KSoft.Debug.UnreachableException(string.Format(
					"Failed to handle build: {0}",
					forBuild));
			}
		}

		void InitializeForHaloReach()
		{
			ImportCodeEnum<Games.HaloReach.RuntimeData.DamageReportingType>("DamageReportingType");
		}
		void InitializeForHalo4()
		{
			ImportCodeEnum<Games.Halo4.RuntimeData.DamageReportingType>("DamageReportingType");
		}
		#endregion

		public void ImportCodeEnum<TEnum>(string enumName)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(!string.IsNullOrEmpty(enumName));
			Contract.Requires(Actions.Count == 0, "Why are you importing an Enum after the DB has been loaded?");

			var script_enum = MegaloScriptEnum.ForEnum<TEnum>(enumName);
			Enums.Add(script_enum);
		}
		public MegaloScriptValueType GetValueType(string name)
		{
			MegaloScriptValueType type;
			if (NameToValueType.TryGetValue(name, out type))
				return type;

			throw new KeyNotFoundException(name);
		}
		public MegaloScriptProtoCondition GetCondition(string conditionName)
		{
			MegaloScriptProtoCondition cond;
			if (NameToConditionMap.TryGetValue(conditionName, out cond))
				return cond;

			throw new KeyNotFoundException(conditionName);
		}
		public MegaloScriptProtoAction GetAction(string actionName)
		{
			MegaloScriptProtoAction action;
			if (NameToActionMap.TryGetValue(actionName, out action))
				return action;

			throw new KeyNotFoundException(actionName);
		}

		internal bool TryGetCondition(string conditionName, out MegaloScriptProtoCondition cond)
		{
			return NameToConditionMap.TryGetValue(conditionName, out cond);
		}
		internal bool TryGetAction(string actionName, out MegaloScriptProtoAction action)
		{
			return NameToActionMap.TryGetValue(actionName, out action);
		}

		public void Postprocess(MegaloStaticDatabase associatedStaticDb, TextWriter errorWriter = null)
		{
			Contract.Requires(associatedStaticDb != null);

			if (errorWriter == null)
				errorWriter = Console.Out;

			this.StaticDatabase = associatedStaticDb;

			#region ValueTypes
			foreach (var type in ValueTypes)
			{
				if (type.BaseType.RequiresBitLength() && type.BitLength == 0)
					errorWriter.WriteLine("ValueType '{0}' doesn't define bitLength", ValueTypeNames[type.NameIndex]);

				switch (type.BaseType)
				{
					case MegaloScriptValueBaseType.Flags:
					{
						var etype = Enums[type.EnumIndex];
						int bc_difference = etype.ValidBitLengthForFlags(type.BitLength);
						if (bc_difference < 0)
							errorWriter.WriteLine("Flags '{0}->{1}' bitLength '{2}' is too small (need {3} more bits)",
								etype.Name, ValueTypeNames[type.NameIndex],
								type.BitLength, -bc_difference);
					} break;
					case MegaloScriptValueBaseType.Enum:
					{
						var etype = Enums[type.EnumIndex];
						int bc_difference = etype.ValidBitLengthForEnum(type.BitLength, type.EnumTraits);
						if (bc_difference < 0)
							errorWriter.WriteLine("Enum '{0}->{1}' bitLength '{2}' is too small (need {3} more bits)",
								etype.Name, ValueTypeNames[type.NameIndex],
								type.BitLength, -bc_difference);
					} break;
					case MegaloScriptValueBaseType.Index:
					{
						if (type.IndexTraits == MegaloScriptValueIndexTraits.PointerRaw)
						{
							int bit_length = type.BitLength;
							if (bit_length != Bits.kByteBitCount && bit_length != Bits.kInt16BitCount && bit_length != Bits.kInt32BitCount)
								errorWriter.WriteLine("Index-type '{0}' is a 'raw' value but doesn't use a natural word size: {1}",
									ValueTypeNames[type.NameIndex], bit_length);
						}
					} break;
				}
			}
			#endregion
			#region Actions
			foreach (var action in Actions)
			{
				var state = new MegaloScriptProtoParamsPostprocessState(errorWriter, action);
				state.Postprocess();
			}
			#endregion
			// #TODO_BLAM: would be nice to dump unused ActionTemplates...as I only copy-pasted H4's in Reach's DB
		}

		#region Variable Sets util
		internal int GetVariableMaxCount(MegaloScriptVariableSet set, MegaloScriptVariableType type)
		{
			return VariableSets[set].Traits[type].MaxCount;
		}
		internal int GetVariableCountBitLength(MegaloScriptVariableSet set, MegaloScriptVariableType type)
		{
			return VariableSets[set].Traits[type].CountBitLength;
		}
		internal int GetVariableIndexBitLength(MegaloScriptVariableSet set, MegaloScriptVariableType type)
		{
			return VariableSets[set].Traits[type].IndexBitLength;
		}
		#endregion

		#region BitStream util
		internal void StreamObjectTypeIndex(IO.BitStream s, ref int objectTypeIndex)
		{ s.StreamIndex(ref objectTypeIndex, Limits.MultiplayerObjectTypes.IndexBitLength); }

		internal int mConditionTypeBitLength;
		internal void StreamConditionType(IO.BitStream s, ref int condType)
		{ s.Stream(ref condType, mConditionTypeBitLength); }
		internal int mActionTypeBitLength;
		internal void StreamActionType(IO.BitStream s, ref int actionType)
		{ s.Stream(ref actionType, mActionTypeBitLength); }

		internal void StreamTriggerCondAndActionRefs(IO.BitStream s,
			ref short firstCondIndex, ref short condCount, ref short firstActionIndex, ref short actionCount)
		{
			if (!Limits.StreamConditionsAndActionsRefsAsRefs)
			{
				s.StreamNoneable(ref firstCondIndex, Limits.Conditions.CountBitLength); // pointer
				s.Stream(ref condCount, Limits.Conditions.CountBitLength);
				s.StreamNoneable(ref firstActionIndex, Limits.Actions.CountBitLength); // pointer
				s.Stream(ref actionCount, Limits.Actions.CountBitLength);
			}
			else
			{
				s.Stream(ref firstCondIndex, Limits.Conditions.IndexBitLength); // reference
				s.Stream(ref condCount, Limits.Conditions.CountBitLength);
				s.Stream(ref firstActionIndex, Limits.Actions.IndexBitLength); // reference
				s.Stream(ref actionCount, Limits.Actions.CountBitLength);
			}
		}

		internal void StreamObjectFilterIndex(IO.BitStream s, ref int objectFilterIndex)
		{ s.StreamIndex(ref objectFilterIndex, Limits.ObjectFilters.IndexBitLength); }
		internal void StreamGameObjectFilterIndex(IO.BitStream s, ref int objectFilterIndex)
		{ s.StreamIndex(ref objectFilterIndex, Limits.GameObjectFilters.IndexBitLength); }
		#endregion
	};
}