#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Flags = {Value}")]
	public sealed partial class MegaloScriptFlagsValue
		: MegaloScriptValueBase
	{
		#region Value
		uint mValue;
		public uint Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptFlagsValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Flags);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptFlagsValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptFlagsValue)other;

			return Value == obj.Value;
		}

		public void ChangeValue(MegaloScriptModel model, string flags)
		{
			//Contract.Requires(!string.IsNullOrEmpty(flags)); // #NOTE_BLAM: we assume null/empty means '0'

			var id_resolving_ctxt = new Proto.MegaloScriptEnum.FlagsNameResolvingContext(model.Database, ValueType);
			Value = Proto.MegaloScriptEnum.FlagsNameResolvingContext.IdResolver(id_resolving_ctxt, flags);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mValue, ValueType.BitLength);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		internal static void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptValueType valueType, ref uint value,
			IO.TagElementNodeType nodeType = IO.TagElementNodeType.Text, string nodeName = null)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Flags);

			if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.UseEnumNames) != 0)
			{
				var id_resolving_ctxt = new Proto.MegaloScriptEnum.FlagsNameResolvingContext(model.Database, valueType);
				var id_resolver = Proto.MegaloScriptEnum.FlagsNameResolvingContext.IdResolver;
				var name_resolver = Proto.MegaloScriptEnum.FlagsNameResolvingContext.NameResolver;

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