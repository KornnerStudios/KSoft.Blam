#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	using MegaloScriptTargetTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptTargetType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptTargetVarValue
		: MegaloScriptValueBase
	{
		#region TargetType
		MegaloScriptTargetType mTargetType;
		public MegaloScriptTargetType TargetType {
			get { return mTargetType; }
			set { mTargetType = value;
				NotifyPropertyChanged(kTargetTypeChanged);
		} }
		#endregion

		#region Value
		MegaloScriptVariableReferenceData mValue = MegaloScriptVariableReferenceData.Null;
		public MegaloScriptVariableReferenceData Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptTargetVarValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.TargetVar);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptTargetVarValue)model.CreateValue(ValueType);
			result.TargetType = TargetType;
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptTargetVarValue)other;

			return TargetType == obj.TargetType && Value.Equals(obj.Value);
		}

		#region SetAs
		public void SetFromVarReference(MegaloScriptVariableReferenceData value)
		{
			Contract.Requires(	value.ReferenceKind == MegaloScriptVariableReferenceType.Team ||
								value.ReferenceKind == MegaloScriptVariableReferenceType.Player);

			switch (value.ReferenceKind)
			{
				case MegaloScriptVariableReferenceType.Team: TargetType = MegaloScriptTargetType.Team; break;
				case MegaloScriptVariableReferenceType.Player: TargetType = MegaloScriptTargetType.Player; break;
			}
			Value = value;
		}
		public void SetAsTeam(MegaloScriptVariableReferenceData team)
		{
			Contract.Requires(team.ReferenceKind == MegaloScriptVariableReferenceType.Team);

			SetFromVarReference(team);
		}
		public void SetAsPlayer(MegaloScriptVariableReferenceData player)
		{
			Contract.Requires(player.ReferenceKind == MegaloScriptVariableReferenceType.Player);

			SetFromVarReference(player);
		}
		#endregion

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mTargetType, 2, MegaloScriptTargetTypeBitStreamer.Instance);

			switch (TargetType)
			{
				case MegaloScriptTargetType.Team: mValue.SerializeTeam(model, s); break;
				case MegaloScriptTargetType.Player: mValue.SerializePlayer(model, s); break;
				case MegaloScriptTargetType.None: mValue = MegaloScriptVariableReferenceData.Null; break;
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnum("targetType", ref mTargetType);

			switch (TargetType)
			{
				case MegaloScriptTargetType.Team: mValue.SerializeTeam(model, s); break;
				case MegaloScriptTargetType.Player: mValue.SerializePlayer(model, s); break;
				case MegaloScriptTargetType.None: mValue = MegaloScriptVariableReferenceData.Null; break;
			}
		}
		#endregion
	};
}