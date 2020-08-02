#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptTokensValue
		: MegaloScriptValueBase
	{
		public int MaxTokens { get { return ValueType.MaxTokens; } }

		public MegaloScriptToken Token0 { get; private set; }
		public MegaloScriptToken Token1 { get; private set; }
		public MegaloScriptToken Token2 { get; private set; }
		// #REVIEW_BLAM: I think there's engine support for up to 4 tokens...haven't seen any instances yet tho
		#region StringIndex
		int mStringIndex = -1;
		public int StringIndex {
			get { return mStringIndex; }
			set { mStringIndex = value;
				NotifyPropertyChanged(kStringIndexChanged);
		} }
		#endregion

		#region TokenCount
		int mTokenCount;
		public int TokenCount {
			get { return mTokenCount; }
			set { mTokenCount = value;
				NotifyPropertyChanged(kTokenCountChanged);
		} }
		#endregion

		public MegaloScriptTokensValue(MegaloScriptModel model, MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Tokens);

			int max_tokens = valueType.MaxTokens;
			if (max_tokens >= 1) Token0 = model.NewToken();
			if (max_tokens >= 2) Token1 = model.NewToken();
			if (max_tokens >= 3) Token2 = model.NewToken();
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptTokensValue)model.CreateValue(ValueType);
			result.StringIndex = StringIndex;
			result.TokenCount = TokenCount;
			if (TokenCount >= 1) result.Token0.CopyFrom(Token0);
			if (TokenCount >= 2) result.Token1.CopyFrom(Token1);
			if (TokenCount >= 3) result.Token2.CopyFrom(Token2);

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptTokensValue)other;

			bool equals = StringIndex == obj.StringIndex && TokenCount == obj.TokenCount;

			if (TokenCount >= 1) equals &= Token0.Equals(obj.Token0);
			if (TokenCount >= 2) equals &= Token0.Equals(obj.Token1);
			if (TokenCount >= 2) equals &= Token0.Equals(obj.Token2);

			return equals;
		}

		void ValidateTokenCount()
		{
			if (TokenCount > ValueType.MaxTokens)
			{
				throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"Value #{0} referencing string #{1} specified too many tokens; {2} > {3}",
					Id, StringIndex, TokenCount, ValueType.MaxTokens));
			}
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			model.MegaloVariant.StreamStringTableIndexPointer(s, ref mStringIndex);
			s.Stream(ref mTokenCount, ValueType.BitLength);

			if (s.IsReading)
			{
				ValidateTokenCount();

				int max_tokens = MaxTokens;
				if (max_tokens >= 1) Token0.Nullify();
				if (max_tokens >= 2) Token1.Nullify();
				if (max_tokens >= 3) Token2.Nullify();
			}

			if (TokenCount >= 1) Token0.Serialize(model, s);
			if (TokenCount >= 2) Token1.Serialize(model, s);
			if (TokenCount >= 3) Token2.Serialize(model, s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			bool reading = s.IsReading;

			model.MegaloVariant.SerializeStringTableIndex(s, "stringIndex", ref mStringIndex);

			if (!s.StreamAttributeOpt("tokenCount", ref mTokenCount, Predicates.IsNotZero) &&
				reading)
				TokenCount = 0;

			if (reading)
			{
				ValidateTokenCount();

				int max_tokens = MaxTokens;
				if (max_tokens >= 1) Token0.Nullify();
				if (max_tokens >= 2) Token1.Nullify();
				if (max_tokens >= 3) Token2.Nullify();
			}

			if (TokenCount >= 1) using (s.EnterCursorBookmark("Token0")) Token0.Serialize(model, s);
			if (TokenCount >= 2) using (s.EnterCursorBookmark("Token1")) Token1.Serialize(model, s);
			if (TokenCount >= 3) using (s.EnterCursorBookmark("Token2")) Token2.Serialize(model, s);
		}
		#endregion
	};
}
