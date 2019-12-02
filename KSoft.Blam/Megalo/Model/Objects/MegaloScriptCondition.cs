using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		protected virtual MegaloScriptCondition NewCondition()
		{
			return new MegaloScriptCondition();
		}

		#region Create
		internal MegaloScriptCondition CreateCondition(int id = TypeExtensions.kNone)
		{
			return CreateObjectPostprocess(Conditions, NewCondition(), id);
		}
		/// <summary>Creates a new condition object and associates it with <paramref name="unionGroupId"/></summary>
		/// <param name="unionGroupId">The preexisting union group, or NONE to create a new one</param>
		/// <returns></returns>
		public MegaloScriptCondition CreateConditionWithUnionGroup(int unionGroupId = TypeExtensions.kNone)
		{
			Contract.Requires(unionGroupId.IsNoneOrPositive());

			var cond = CreateCondition();

			var union_group = unionGroupId.IsNone() ? CreateUnionGroup() : UnionGroups[unionGroupId];
			union_group.Add(cond);

			return cond;
		}
		/// <summary>Creates a new condition object and associates it with <paramref name="unionGroupId"/></summary>
		/// <param name="unionGroupId">The preexisting union group</param>
		/// <param name="evaluationBefore">Condition in the union group to evaluate this condition before, or NONE to just append</param>
		/// <returns></returns>
		public MegaloScriptCondition CreateConditionInUnionGroup(int unionGroupId, MegaloScriptModelObjectHandle evaluationBefore)
		{
			Contract.Requires(unionGroupId.IsNotNone());
			Contract.Requires(evaluationBefore.Type == MegaloScriptModelObjectType.Condition);
			Contract.Requires(evaluationBefore.IsNoneOrPositive);

			var cond = CreateCondition();

			var union_group = UnionGroups[unionGroupId];
			union_group.Insert(cond, evaluationBefore);

			return cond;
		}
		/// <summary>Creates a new condition object and associates it with <paramref name="unionGroupId"/></summary>
		/// <param name="unionGroupId">The preexisting union group</param>
		/// <returns></returns>
		/// <remarks>Appends the condition to the end of the end of the evaluation list</remarks>
		public MegaloScriptCondition CreateConditionInUnionGroup(int unionGroupId)
		{
			Contract.Requires(unionGroupId.IsNotNone());

			return CreateConditionInUnionGroup(unionGroupId, MegaloScriptModelObjectHandle.NullCondition);
		}
		#endregion

		internal void HandleRemoval(MegaloScriptCondition cond)
		{
			var union_group = UnionGroups[cond.UnionGroup];
			union_group.Remove(this, cond.Handle);

			HandleRemoval(cond.Arguments);
		}

		#region NewFrom* Utils
		static readonly Func<IO.BitStream, MegaloScriptModel, MegaloScriptCondition> NewConditionFromBitStream =
			(s, model) => model.CreateCondition();
		static MegaloScriptCondition NewConditionFromTagStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model)
			where TDoc : class
			where TCursor : class
			{ return model.NewCondition(); }
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptCondition
		: MegaloScriptModelObjectWithParameters
	{
		public override MegaloScriptModelObjectType ObjectType { get { return MegaloScriptModelObjectType.Condition; } }
		public Proto.MegaloScriptProtoCondition ProtoData { get; private set; }

		#region Inverted
		bool mInverted;
		public bool Inverted {
			get { return mInverted; }
			set { mInverted = value;
				NotifyPropertyChanged(kInvertedChanged);
		} }
		#endregion
		// both of these are local to owning (virtual) trigger when encoded
		#region UnionGroup
		int mUnionGroup = TypeExtensions.kNone;
		public int UnionGroup {
			get { return mUnionGroup; }
			private set { mUnionGroup = value;
				NotifyPropertyChanged(kUnionGroupChanged);
		} }

		internal void AssociateWith(MegaloScriptUnionGroup unionGroup)
		{
			UnionGroup = unionGroup.Id;
		}
		#endregion
		/// <remarks>Only used during de/compiling</remarks>
		internal int ExecuteBeforeAction = TypeExtensions.kNone;

		internal void InitializeForType(MegaloScriptModel model, int condType)
		{
			Contract.Requires(condType >= 0 && condType < model.Database.Conditions.Count);

			ProtoData = model.Database.Conditions[condType];
			NotifyPropertyChanged(kProtoDataChanged);
			Arguments = new MegaloScriptArguments(model, ProtoData);
		}
		internal void InitializeForType(MegaloScriptModel model, string condType)
		{
			var proto_cond = model.Database.GetCondition(condType);

			ProtoData = proto_cond;
			NotifyPropertyChanged(kProtoDataChanged);
			Arguments = new MegaloScriptArguments(model, ProtoData);
		}

		#region IBitStreamSerializable Members
		protected virtual int SerializeImpl(MegaloScriptModel model, IO.BitStream s)
		{
			int type = s.IsReading ? TypeExtensions.kNone : ProtoData.DBID;
			model.Database.StreamConditionType(s, ref type);
			if (type != 0)
			{
				s.Stream(ref mInverted);
				model.StreamLocalUnionGroupIndex(s, ref mUnionGroup);
				s.Stream(ref ExecuteBeforeAction, model.Database.Limits.Actions.IndexBitLength);
			}
			return type;
		}
		public sealed override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			int type = SerializeImpl(model, s);

			if (s.IsReading)
			{
				InitializeForType(model, type);
				Arguments.InitializeEmptyValues(model);
			}

			Arguments.Serialize(model, s);
		}
		#endregion

		// #NOTE_BLAM: Can be embedded via MegaloScriptModelObjectHandle. Attributes must not conflict with that type's
		#region ITagElementStringNameStreamable Members
		const string kAttributeNameUnionGroup = "unionGroupID";
		internal const int kUseNewUnionGroupId = TypeExtensions.kNone - 1;
		internal const int kUsePrevUnionGroupId = TypeExtensions.kNone - 2;
		bool UnionGroupIdIsValid(MegaloScriptModel model, bool reading)
		{
			bool valid = true;

			if (UnionGroup.IsNone())
				valid = false;
			else if (reading && model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds() &&
					 (UnionGroup == kUsePrevUnionGroupId || UnionGroup == kUseNewUnionGroupId))
				valid = true;
			else if (UnionGroup < 0 || model.UnionGroups.SlotIsFree(UnionGroup))
				valid = false;

			return valid;
		}

		protected void WriteUnionGroupForSansIds<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var union_group = model.UnionGroups[UnionGroup];

			int union_locality = union_group.IndexOf(this.Handle);
			//s.WriteAttribute(kAttributeNameUnionGroup, union_locality > 0 ? kUsePrevUnionGroupId : kUseNewUnionGroupId);
			if (union_locality > 0)
				s.WriteAttribute(kAttributeNameUnionGroup, kUsePrevUnionGroupId);
		}
		protected virtual int SerializeImpl<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			int type = s.IsReading ? TypeExtensions.kNone : ProtoData.DBID;
			model.SerializeOperationId(s, ref type, ObjectType);

			if (type != 0)
			{
				if (s.IsWriting && (model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.TryToPort) != 0)
					ProtoData.WriteForTryToPort(s, model.Database);

				if (s.IsWriting && (model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.WriteConditionTypeNames) != 0)
					s.WriteAttribute("name", ProtoData.Name);

				#region UnionGroup
				if (model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds())
				{
					if (s.IsWriting)
						WriteUnionGroupForSansIds(model, s);
					else if (!s.ReadAttributeOpt(kAttributeNameUnionGroup, ref mUnionGroup))
						mUnionGroup = kUseNewUnionGroupId;
				}
				else
					s.StreamAttribute(kAttributeNameUnionGroup, ref mUnionGroup);
				#endregion

				s.StreamAttributeOpt("invert", ref mInverted, Predicates.IsTrue);

				if (!UnionGroupIdIsValid(model, s.IsReading))
					throw new System.IO.InvalidDataException(string.Format("Condition #{0} has an invalid union group id #{1}",
						Id, UnionGroup));
			}
			return type;
		}
		public sealed override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			SerializeIdOpt(model, s);
			int type = SerializeImpl(model, s);
			SerializeCommentOut(s);

			if (s.IsReading)
				InitializeForType(model, type);

			Arguments.Serialize(model, s);
		}
		#endregion
	};
}