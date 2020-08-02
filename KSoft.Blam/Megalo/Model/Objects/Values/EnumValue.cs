#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Enum = {Value}")]
	public sealed partial class MegaloScriptEnumValue
		: MegaloScriptValueBase
	{
		#region Value
		int mValue;
		public int Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptEnumValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Enum);

			if (valueType.EnumTraits == Proto.MegaloScriptValueEnumTraits.HasNoneMember)
				mValue = -1;
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptEnumValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptEnumValue)other;

			return Value == obj.Value;
		}

		public void ChangeValue(MegaloScriptModel model, string enumMemberName)
		{
			Contract.Requires(!string.IsNullOrEmpty(enumMemberName));

			var id_resolving_ctxt = new Proto.MegaloScriptEnum.EnumNameResolvingContext(model.Database, ValueType);
			Value = Proto.MegaloScriptEnum.EnumNameResolvingContext.IdResolver(id_resolving_ctxt, enumMemberName);
		}

		#region IBitStreamSerializable Members
		internal static void SerializeValue(MegaloScriptModel model, IO.BitStream s,
			MegaloScriptValueType valueType, ref int value)
		{
			if (valueType.EnumTraits == Proto.MegaloScriptValueEnumTraits.HasNoneMember)
				s.StreamNoneable(ref value, valueType.BitLength);
			else
				s.Stream(ref value, valueType.BitLength);

			if (s.IsReading && !model.EnumIndexIsValid(valueType, value))
			{
				throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0} doesn't have a #{1} member",
					model.Database.Enums[valueType.EnumIndex].Name, value));
			}
		}
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			SerializeValue(model, s, ValueType, ref mValue);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		internal static void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptValueType valueType, ref int value,
			IO.TagElementNodeType nodeType = IO.TagElementNodeType.Text, string nodeName = null)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Enum);

			if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.UseEnumNames) != 0)
			{
				var id_resolving_ctxt = new Proto.MegaloScriptEnum.EnumNameResolvingContext(model.Database, valueType);
				var id_resolver = Proto.MegaloScriptEnum.EnumNameResolvingContext.IdResolver;
				var name_resolver = Proto.MegaloScriptEnum.EnumNameResolvingContext.NameResolver;

				switch (nodeType)
				{
					case IO.TagElementNodeType.Element:
						s.StreamElementIdAsString(nodeName,		ref value, id_resolving_ctxt, id_resolver, name_resolver);
						break;
					case IO.TagElementNodeType.Attribute:
						s.StreamAttributeIdAsString(nodeName,	ref value, id_resolving_ctxt, id_resolver, name_resolver);
						break;
					case IO.TagElementNodeType.Text:
						s.StreamCursorIdAsString(				ref value, id_resolving_ctxt, id_resolver, name_resolver);
						break;
				}
			}
			else
			{
				switch (nodeType)
				{
					case IO.TagElementNodeType.Element:		s.StreamElement(nodeName,	ref value); break;
					case IO.TagElementNodeType.Attribute:	s.StreamAttribute(nodeName,	ref value); break;
					case IO.TagElementNodeType.Text:		s.StreamCursor(				ref value); break;
				}
			}
		}

		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			SerializeValue(model, s, ValueType, ref mValue);
		}
		#endregion
	};
}
