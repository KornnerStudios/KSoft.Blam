using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.RuntimeData.Variants
{
	partial class GameEngineMegaloVariant
	{
		public int CreateUserDefinedOption(string codeName, int nameStringIndex, int descStringIndex)
		{
			if (UserDefinedOptions.Count == UserDefinedOptions.Capacity)
				return TypeExtensions.kNone;

			var option = new MegaloVariantUserDefinedOption();
			option.CodeName = codeName;
			option.NameStringIndex = nameStringIndex;
			option.DescriptionStringIndex = descStringIndex;

			UserDefinedOptions.Add(option);
			return UserDefinedOptions.Count - 1;
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	internal struct MegaloVariantUserDefinedOptionValueParams
	{
		public static readonly MegaloVariantUserDefinedOptionValueParams Null = new MegaloVariantUserDefinedOptionValueParams()
		{
			NameStringIndex=-1,
			DescriptionStringIndex=-1,
			Value=-1
		};

		public int NameStringIndex;
		public int DescriptionStringIndex;
		public int Value;
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloVariantUserDefinedOptionValueElement
		: IO.ITagElementStringNameStreamable
	{
		public int Value;
		public int NameStringIndex = TypeExtensions.kNone, DescriptionStringIndex = TypeExtensions.kNone; // sbytes at runtime

		public MegaloVariantUserDefinedOptionValueElement()
		{
		}
		internal MegaloVariantUserDefinedOptionValueElement(MegaloVariantUserDefinedOptionValueParams valueParams)
		{
			Value = valueParams.Value;
			NameStringIndex = valueParams.NameStringIndex;
			DescriptionStringIndex = valueParams.DescriptionStringIndex;
		}

		public void Serialize(IO.BitStream s, GameEngineMegaloVariant megalo, bool isRangeValue)
		{
			s.Stream(ref Value, 10, signExtend:true);
			if (!isRangeValue)
			{
				megalo.StreamStringTableIndexReference(s, ref NameStringIndex);
				megalo.StreamStringTableIndexReference(s, ref DescriptionStringIndex);
			}
		}

		#region ITagElementStringNameStreamable Members
		internal void SerializeValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("value", ref Value);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var variant = (GameEngineMegaloVariant)s.Owner;

			variant.SerializeStringTableIndex(s, "nameIndex", ref NameStringIndex);
			variant.SerializeStringTableIndex(s, "descIndex", ref DescriptionStringIndex);
			SerializeValue(s);
		}
		#endregion
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloVariantUserDefinedOptionValue
	{
		public int NameStringIndex = TypeExtensions.kNone, DescriptionStringIndex = TypeExtensions.kNone; // sbyte at runtime
		public bool IsRangeValue;
		public int DefaultValueIndex = TypeExtensions.kNone;
		public int Value = TypeExtensions.kNone;
		public List<MegaloVariantUserDefinedOptionValueElement> Values { get; private set; }

		#region Range values
		public MegaloVariantUserDefinedOptionValueElement RangeMinValue { get {
			Contract.Requires(IsRangeValue);
			return Values[0];
		} }
		public MegaloVariantUserDefinedOptionValueElement RangeMaxValue { get {
			Contract.Requires(IsRangeValue);
			return Values[1];
		} }
		#endregion

		public MegaloVariantUserDefinedOptionValue()
		{
			Values = new List<MegaloVariantUserDefinedOptionValueElement>();
		}

		public void Clear()
		{
			IsRangeValue = false;
			Values.Clear();
		}

		#region Add/Set value interfaces
		internal void SetRange(int defaultValue,
			MegaloVariantUserDefinedOptionValueParams min, MegaloVariantUserDefinedOptionValueParams max)
		{
			IsRangeValue = true;
			Value = defaultValue;

			Values.Clear();
			Values.Add(new MegaloVariantUserDefinedOptionValueElement(min));
			Values.Add(new MegaloVariantUserDefinedOptionValueElement(max));
		}
		internal void SetRange(int defaultValue,
			int min, int max)
		{
			IsRangeValue = true;
			Value = defaultValue;

			Values.Clear();
			Values.Add(new MegaloVariantUserDefinedOptionValueElement());
			Values.Add(new MegaloVariantUserDefinedOptionValueElement());
			RangeMinValue.Value = min;
			RangeMaxValue.Value = max;
		}
		internal void SetValues(int defaultValueIndex,
			params MegaloVariantUserDefinedOptionValueParams[] values)
		{
			IsRangeValue = false;
			DefaultValueIndex = defaultValueIndex;

			Values.Clear();
			foreach (var vp in values)
				Values.Add(new MegaloVariantUserDefinedOptionValueElement(vp));
		}
		internal void AddValue(MegaloVariantUserDefinedOptionValueParams valueParam, bool isDefault)
		{
			if (isDefault)
				DefaultValueIndex = Values.Count;

			Values.Add(new MegaloVariantUserDefinedOptionValueElement(valueParam));
		}
		#endregion

		#region IBitStreamSerializable Members
		void ValuesRead(IO.BitStream s, GameEngineMegaloVariant megalo, int count, bool isRangeValue)
		{
			if (count.IsNone())
				megalo.StreamUserDefinedValuesCount(s, ref count);

			for (int x = 0; x < count; x++)
			{
				var e = new MegaloVariantUserDefinedOptionValueElement();
				e.Serialize(s, megalo, isRangeValue);
				Values.Add(e);
			}
		}
		void ValuesWrite(IO.BitStream s, GameEngineMegaloVariant megalo, bool isRangeValue)
		{
			for (int x = 0; x < Values.Count; x++)
				Values[x].Serialize(s, megalo, isRangeValue);
		}
		public void Serialize(IO.BitStream s, GameEngineMegaloVariant megalo)
		{
			megalo.StreamStringTableIndexReference(s, ref NameStringIndex);
			megalo.StreamStringTableIndexReference(s, ref DescriptionStringIndex);
			s.Stream(ref IsRangeValue);
			if (IsRangeValue)
			{
				s.Stream(ref Value, 10, signExtend:true);
				// min, max
					 if (s.IsReading) ValuesRead(s, megalo, 2, IsRangeValue);
				else if (s.IsWriting) ValuesWrite(s, megalo, IsRangeValue);
			}
			else
			{
				megalo.StreamUserDefinedValueIndex(s, ref DefaultValueIndex);
				if (s.IsReading) ValuesRead(s, megalo, TypeExtensions.kNone, IsRangeValue);
				else if (s.IsWriting)
				{
					int count = Values.Count;
					megalo.StreamUserDefinedValuesCount(s, ref count);

					ValuesWrite(s, megalo, IsRangeValue);
				}
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		const string kValuesElementName = "Values";

		internal void SerializeRangeValues<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (!IsRangeValue)
				return; // #HACK: IgnoreWritePredicates hack!

			s.StreamAttribute("rangeValue", ref Value);
			if (s.IsReading)
			{	Values.Add(new MegaloVariantUserDefinedOptionValueElement());
				Values.Add(new MegaloVariantUserDefinedOptionValueElement());
			}
			using (s.EnterCursorBookmark(kValuesElementName))
			{
				using (s.EnterCursorBookmark("Min")) RangeMinValue.SerializeValue(s);
				using (s.EnterCursorBookmark("Max")) RangeMaxValue.SerializeValue(s);
			}
		}
		internal void SerializeValues<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark(kValuesElementName))
			{
				s.StreamAttribute("defaultIndex", ref DefaultValueIndex);
				s.StreamableElements("entry", Values);
			}
		}
		internal void SerializeHeader<TDoc, TCursor>(GameEngineMegaloVariant variant, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			variant.SerializeStringTableIndex(s, "nameIndex", ref NameStringIndex);
			variant.SerializeStringTableIndex(s, "descIndex", ref DescriptionStringIndex);
			if (s.StreamAttributeOpt("isRangeValue", ref IsRangeValue, Predicates.IsTrue))
				SerializeRangeValues(s);
			else
				SerializeValues(s);
		}
		#endregion
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloVariantUserDefinedOption
		: Blam.Megalo.Model.MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public MegaloVariantUserDefinedOptionValue ValueData = new MegaloVariantUserDefinedOptionValue();
		public int ValueIndex = TypeExtensions.kNone;
		public int Value = TypeExtensions.kNone;

		public int NameStringIndex {
			get { return ValueData.NameStringIndex; }
			set { ValueData.NameStringIndex = value; }
		}
		public int DescriptionStringIndex {
			get { return ValueData.DescriptionStringIndex; }
			set { ValueData.DescriptionStringIndex = value; }
		}

		#region Add/Set value interfaces
		internal void SetRange(int defaultValue,
			MegaloVariantUserDefinedOptionValueParams min, MegaloVariantUserDefinedOptionValueParams max)
		{
			Value = defaultValue;

			ValueData.SetRange(defaultValue, min, max);
		}
		internal void SetRange(int defaultValue,
			int min, int max)
		{
			Value = defaultValue;

			ValueData.SetRange(defaultValue, min, max);
		}
		internal void SetValues(int defaultValueIndex,
			params MegaloVariantUserDefinedOptionValueParams[] values)
		{
			ValueIndex = defaultValueIndex;

			ValueData.SetValues(defaultValueIndex, values);
		}
		internal void AddValue(MegaloVariantUserDefinedOptionValueParams valueParam, bool isDefault = false)
		{
			if (isDefault)
				ValueIndex = ValueData.Values.Count;

			ValueData.AddValue(valueParam, isDefault);
		}
		internal void CopyValueFrom(MegaloVariantUserDefinedOption other, int otherIndex = -1, bool isDefault = false)
		{
			if (otherIndex.IsNone())
				otherIndex = other.ValueData.Values.Count-1;

			if (isDefault)
				ValueIndex = ValueData.Values.Count;

			var other_value = other.ValueData.Values[otherIndex];
			ValueData.AddValue(new MegaloVariantUserDefinedOptionValueParams()
				{	NameStringIndex = other_value.NameStringIndex,
					DescriptionStringIndex = other_value.DescriptionStringIndex,
					Value = other_value.Value,
				}, isDefault);
		}
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			var megalo = (GameEngineMegaloVariant)s.Owner;

			ValueData.Serialize(s, megalo);
			if (ValueData.IsRangeValue)
				s.Stream(ref Value, 10, signExtend:true);
			else
				megalo.StreamUserDefinedValueIndex(s, ref ValueIndex);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var variant = (GameEngineMegaloVariant)s.Owner;
			ValueData.SerializeHeader(variant, s);

			SerializeCodeName(s);

			if (ValueData.IsRangeValue)
				s.StreamAttribute("value", ref Value);
			else
				s.StreamAttribute("valueIndex", ref ValueIndex);
		}
		#endregion
	};
}