namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class MegaloScriptVarReferenceValueBase
		: MegaloScriptValueBase
	{
		// #TODO_BLAM: need to validate the value Var is changing to is of the same type we're expecting,
		// assuming the ValueType isn't Any.
		#region Var
		MegaloScriptVariableReferenceData mVar = MegaloScriptVariableReferenceData.Null;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mVar), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData Var { get; set; }
		#endregion

		protected MegaloScriptVarReferenceValueBase(MegaloScriptValueType valueType) : base(valueType)
		{
		}

		protected void CopyVarTo(MegaloScriptVarReferenceValueBase result)
		{
			result.Var = Var;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptVarReferenceValueBase>(other);

			return Var.Equals(obj.Var);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			var type = ValueType.VarReference;

			switch (type)
			{
				case MegaloScriptVarReferenceType.Custom:	mVar.SerializeCustom(model, s); break;
				case MegaloScriptVarReferenceType.Player:	mVar.SerializePlayer(model, s); break;
				case MegaloScriptVarReferenceType.Object:	mVar.SerializeObject(model, s); break;
				case MegaloScriptVarReferenceType.Team:		mVar.SerializeTeam(model, s); break;
				case MegaloScriptVarReferenceType.Timer:	mVar.SerializeTimer(model, s); break;
				case MegaloScriptVarReferenceType.Any:		mVar.Serialize(model, s); break;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var type = ValueType.VarReference;

			switch (type)
			{
				case MegaloScriptVarReferenceType.Custom:	mVar.SerializeCustom(model, s); break;
				case MegaloScriptVarReferenceType.Player:	mVar.SerializePlayer(model, s); break;
				case MegaloScriptVarReferenceType.Object:	mVar.SerializeObject(model, s); break;
				case MegaloScriptVarReferenceType.Team:		mVar.SerializeTeam(model, s); break;
				case MegaloScriptVarReferenceType.Timer:	mVar.SerializeTimer(model, s); break;
				case MegaloScriptVarReferenceType.Any:		mVar.Serialize(model, s); break;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloScriptVarReferenceValue
		: MegaloScriptVarReferenceValueBase
	{
		public MegaloScriptVarReferenceValue(MegaloScriptValueType valueType) : base(valueType)
		{
			ThrowIfUnexpectedBaseType(valueType, MegaloScriptValueBaseType.VarReference);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptVarReferenceValue)model.CreateValue(ValueType);
			CopyVarTo(result);

			return result;
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptObjectReferenceWithPlayerVarIndexValue
		: MegaloScriptVarReferenceValueBase
	{
		#region PlayerVarIndex
		int mPlayerVarIndex;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mPlayerVarIndex), AlwaysNotify = true)]
		public partial int PlayerVarIndex { get; set; }
		#endregion

		public MegaloScriptObjectReferenceWithPlayerVarIndexValue(MegaloScriptValueType valueType) : base(valueType)
		{
			ThrowIfUnexpectedBaseType(valueType, MegaloScriptValueBaseType.ObjectReferenceWithPlayerVarIndex);

			Var = MegaloScriptVariableReferenceData.Object;
			PlayerVarIndex = TypeExtensions.kNone;
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptObjectReferenceWithPlayerVarIndexValue)model.CreateValue(ValueType);
			CopyVarTo(result);
			result.PlayerVarIndex = PlayerVarIndex;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			return base.ValueEquals(other) &&
				PlayerVarIndex == (KSoft.Debug.TypeCheck.CastReference<MegaloScriptObjectReferenceWithPlayerVarIndexValue>(other)).PlayerVarIndex;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			base.Serialize(model, s);

			model.Database.ObjectReferenceWithPlayerVarIndex.StreamPlayerVarIndex(s, ref mPlayerVarIndex,
				Var.Type, model);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			using (s.EnterCursorBookmark("Object"))
			{
				base.SerializeValue(model, s);
			}

			model.Database.ObjectReferenceWithPlayerVarIndex.StreamPlayerVarIndex(s, ref mPlayerVarIndex,
				Var.Type, model);
		}
		#endregion
	};
}
