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
		protected virtual MegaloScriptAction NewAction()
		{
			return new MegaloScriptAction();
		}

		public MegaloScriptAction CreateAction(int id = TypeExtensions.kNone)
		{
			return CreateObjectPostprocess(Actions, NewAction(), id);
		}

		#region NewFrom* Utils
		static readonly Func<IO.BitStream, MegaloScriptModel, MegaloScriptAction> NewActionFromBitStream =
			(s, model) => model.CreateAction();
		static MegaloScriptAction NewActionFromTagStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model)
			where TDoc : class
			where TCursor : class
			{ return model.NewAction(); }
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptAction
		: MegaloScriptModelObjectWithParameters
	{
		public override MegaloScriptModelObjectType ObjectType { get { return MegaloScriptModelObjectType.Action; } }
		public Proto.MegaloScriptProtoAction ProtoData { get; private set; }

		internal void InitializeForType(MegaloScriptModel model, int actionType)
		{
			Contract.Requires(actionType >= 0 && actionType < model.Database.Actions.Count);

			ProtoData = model.Database.Actions[actionType];
			NotifyPropertyChanged(kProtoDataChanged);
			Arguments = new MegaloScriptArguments(model, ProtoData);
		}
		internal void InitializeForType(MegaloScriptModel model, string actionType)
		{
			var proto_action = model.Database.GetAction(actionType);

			ProtoData = proto_action;
			NotifyPropertyChanged(kProtoDataChanged);
			Arguments = new MegaloScriptArguments(model, ProtoData);
		}

		#region IBitStreamSerializable Members
		protected virtual int SerializeImpl(MegaloScriptModel model, IO.BitStream s)
		{
			int type = s.IsReading ? TypeExtensions.kNone : ProtoData.DBID;
			model.Database.StreamActionType(s, ref type);

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
		protected virtual int SerializeImpl<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			int type = s.IsReading ? TypeExtensions.kNone : ProtoData.DBID;
			model.SerializeOperationId(s, ref type, ObjectType);

			if (type != 0 && s.IsWriting)
			{
				if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.TryToPort) != 0)
					ProtoData.WriteForTryToPort(s, model.Database);

				if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.WriteActionTypeNames) != 0)
					s.WriteAttribute("name", ProtoData.Name);
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