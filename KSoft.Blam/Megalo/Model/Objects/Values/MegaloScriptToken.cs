using System;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		internal abstract MegaloScriptToken NewToken();
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1067:Override Equals when implementing IEquatable")]
	public abstract partial class MegaloScriptToken
		: IEquatable<MegaloScriptToken>
	{
		#region Type
		MegaloScriptTokenAbstractType mType = MegaloScriptTokenAbstractType.None;
		public MegaloScriptTokenAbstractType Type {
			get { return mType; }
			set { mType = value;
				NotifyPropertyChanged(kTypeChanged);
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

		public void Nullify()
		{
			Type = MegaloScriptTokenAbstractType.None;
			Value = MegaloScriptVariableReferenceData.Null;
		}

		public virtual void CopyFrom(MegaloScriptToken other)
		{
			Type = other.mType;
			Value = other.Value;
		}

		#region IBitStreamSerializable Members
		protected abstract void SerializeType(MegaloScriptModel model, IO.BitStream s,
			ref MegaloScriptTokenAbstractType abstractType);

		public void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			SerializeType(model, s, ref mType);

			switch (Type)
			{
				case MegaloScriptTokenAbstractType.Player:			mValue.SerializePlayer(model, s); break;
				case MegaloScriptTokenAbstractType.Team:			mValue.SerializeTeam(model, s); break;
				case MegaloScriptTokenAbstractType.Object:			mValue.SerializeObject(model, s); break;
				case MegaloScriptTokenAbstractType.Numeric:
				case MegaloScriptTokenAbstractType.SignedNumeric:	mValue.SerializeCustom(model, s); break;
				case MegaloScriptTokenAbstractType.Timer:			mValue.SerializeTimer(model, s); break;

				default: throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("type", ref mType);

			switch (Type)
			{
				case MegaloScriptTokenAbstractType.Player:			mValue.SerializePlayer(model, s); break;
				case MegaloScriptTokenAbstractType.Team:			mValue.SerializeTeam(model, s); break;
				case MegaloScriptTokenAbstractType.Object:			mValue.SerializeObject(model, s); break;
				case MegaloScriptTokenAbstractType.Numeric:
				case MegaloScriptTokenAbstractType.SignedNumeric:	mValue.SerializeCustom(model, s); break;
				case MegaloScriptTokenAbstractType.Timer:			mValue.SerializeTimer(model, s); break;

				default: throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}
		#endregion

		#region IEquatable<MegaloScriptToken> Members
		public bool Equals(MegaloScriptToken other)
		{
			return Type == other.Type && Value.Equals(other.Value);
		}
		#endregion
	};
}
