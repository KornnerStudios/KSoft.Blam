#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Target = {ValueType.IndexTarget}, Value = {Value}")]
	public sealed partial class MegaloScriptIndexValue
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

		public MegaloScriptIndexValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Index);

			if (valueType.IndexTraits != Proto.MegaloScriptValueIndexTraits.Reference)
				mValue = -1;
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptIndexValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptIndexValue)other;

			return Value == obj.Value;
		}

		public bool ValidateValue(MegaloScriptModel model)
		{
			return model.IndexTargetIsValid(ValueType.IndexTarget, ValueType.IndexTraits, Value);
		}

		public void ChangeValue(MegaloScriptModel model, string indexName)
		{
			Contract.Requires(!string.IsNullOrEmpty(indexName));

			Value = model.GetTargetIndexFromName(ValueType.IndexTarget, indexName);
		}

		#region IBitStreamSerializable Members
		internal static void SerializeValue(MegaloScriptModel model, IO.BitStream s,
			MegaloScriptValueType valueType, ref int value)
		{
			var traits = valueType.IndexTraits;
			int bit_length = valueType.BitLength;

			int local_value = value;
			if (s.IsWriting && valueType.IndexTarget == Proto.MegaloScriptValueIndexTarget.Trigger)
				model.mCompilerState.RemapTriggerReference(ref local_value);

			switch (traits)
			{
				case Proto.MegaloScriptValueIndexTraits.PointerHasValue:
					s.StreamIndex(ref local_value, bit_length);
					break;
				case Proto.MegaloScriptValueIndexTraits.Pointer:
					s.StreamNoneable(ref local_value, bit_length);
					break;
				case Proto.MegaloScriptValueIndexTraits.PointerRaw:
					s.Stream(ref local_value, bit_length, signExtend: true);
					break;
				case Proto.MegaloScriptValueIndexTraits.Reference:
					s.Stream(ref local_value, bit_length);
					break;
				default: throw new KSoft.Debug.UnreachableException(traits.ToString());
			}

			if(s.IsReading)
				value = local_value;
		}
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			SerializeValue(model, s, ValueType, ref mValue);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		static void SerializeTriggerReferenceValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptValueType valueType, ref int value)
			where TDoc : class
			where TCursor : class
		{
			var handle = s.IsWriting ? new MegaloScriptModelObjectHandle(MegaloScriptModelObjectType.Trigger, value) : MegaloScriptModelObjectHandle.Null;

			if (s.IsWriting)
			{
				Contract.Assert(model.Triggers[handle.Id].TriggerType == MegaloScriptTriggerType.InnerLoop);
			}
			using (s.EnterCursorBookmark("T")) // have to nest or MegaloScriptModelObjectHandle will overwrite our Param ID with the Trigger's
				MegaloScriptModelObjectHandle.SerializeForEmbed(s, model, ref handle);

			if (s.IsReading)
				value = handle.Id;
		}
		internal static void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptValueType valueType, ref int value,
			IO.TagElementNodeType nodeType = IO.TagElementNodeType.Text, string nodeName = null)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Index);

			var target = valueType.IndexTarget;

			if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.EmbedObjects) != 0 &&
				target == MegaloScriptValueIndexTarget.Trigger)
			{
				SerializeTriggerReferenceValue(model, s, valueType, ref value);
			}
			#region UseIndexNames
			else if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.UseIndexNames) != 0 &&
				target.HasIndexName())
			{
				var id_resolving_ctxt = new MegaloScriptModel.IndexNameResolvingContext(model, target);
				var id_resolver = MegaloScriptModel.IndexNameResolvingContext.IdResolver;
				var name_resolver = MegaloScriptModel.IndexNameResolvingContext.NameResolver;

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
			#endregion
			else
			{
				switch (nodeType)
				{
					case IO.TagElementNodeType.Element:		s.StreamElement(nodeName,	ref value); break;
					case IO.TagElementNodeType.Attribute:	s.StreamAttribute(nodeName,	ref value); break;
					case IO.TagElementNodeType.Text:		s.StreamCursor(				ref value); break;
				}
			}

			// #REVIEW_BLAM: this will fail when embedding and the target is a Trigger since we don't preload triggers or such
			if (s.IsReading &&
				((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.EmbedObjects) == 0 || target != MegaloScriptValueIndexTarget.Trigger)
				)
			{
				if (!model.IndexTargetIsValid(target, valueType.IndexTraits, value))
				{
					s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
						"A {0} reference has an invalid value {1}", target, value)));
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