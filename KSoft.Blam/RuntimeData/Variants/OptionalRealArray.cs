using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class OptionalRealArrayInfo
	{
		public readonly int kLength;
		public readonly int kBitCount;
		public readonly float kRangeMin, kRangeMax;
		public readonly bool kSigned, kUnknown;
		public Type kValuesEnum;

		public OptionalRealArrayInfo(int len, int bits = 16, float min = -200.0f, float max = 200.0f, bool signed = true, bool unknown = true,
			Type valuesEnum = null)
		{
			kLength = len;
			kBitCount = bits;
			kRangeMin = min;
			kRangeMax = max;
			kSigned = signed;
			kUnknown = unknown;
			kValuesEnum = valuesEnum;
		}
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class OptionalRealArray
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		readonly OptionalRealArrayInfo kInfo;
		uint mValidFlags;
		readonly float[] mArray;

		public int Length { get { return mArray.Length; } }
		public bool HasValues { get { return mValidFlags != 0; } }
		internal bool AreUnchanged { get { return mValidFlags == 0; } }

		internal OptionalRealArray(OptionalRealArrayInfo info)
		{
			kInfo = info;
			mArray = new float[kInfo.kLength];
		}

		void SetImpl(int index, float? value)
		{
			uint old_flags = mValidFlags;
			float? old_value = this[index];

			Bitwise.Flags.Modify(value.HasValue, ref mValidFlags, 1U << index);
			mArray[index] = value.GetValueOrDefault();

			if (old_flags != mValidFlags)
				NotifyPropertyChanged(kHasValuesChanged);

			NotifyItemChanged(index, old_value, value);
		}
		public void Clear(int index)
		{
			Contract.Requires(index >= 0 && index < Length);

			float? old_value = this[index];
			if (old_value.HasValue)
				SetImpl(index, (float?)null);
		}

		public float? this[int index] {
			get { Contract.Requires(index >= 0 && index < Length);
				return Bitwise.Flags.Test(mValidFlags, 1U << index)
					? mArray[index]
					: (float?)null;
			}
			set { Contract.Requires(index >= 0 && index < Length);
				SetImpl(index, value);
		} }

		#region IBitStreamSerializable Members
		void Read(IO.BitStream bs)
		{
			for (int x = 0, mask = 1; x < kInfo.kLength; x++, mask <<= 1)
			{
				bool has_value;
				bs.Read(out has_value);

				if (has_value)
				{
					mValidFlags |= (uint)mask;
					bs.Stream(ref mArray[x],
						kInfo.kRangeMin, kInfo.kRangeMax, kInfo.kBitCount, kInfo.kSigned, kInfo.kUnknown);
				}
			}
		}
		void Write(IO.BitStream bs)
		{
			for (int x = 0, mask = 1; x < kInfo.kLength; x++, mask <<= 1)
			{
				bool has_value = (mValidFlags & (uint)mask) != 0;
				bs.Write(has_value);
				if (has_value)
					bs.Stream(ref mArray[x],
						kInfo.kRangeMin, kInfo.kRangeMax, kInfo.kBitCount, kInfo.kSigned, kInfo.kUnknown);
			}
		}
		public void Serialize(IO.BitStream s)
		{
				 if (s.IsReading) Read(s);
			else if (s.IsWriting) Write(s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		const string kEntryElementName = "entry";
		const string kEntryAttrKeyName = "key";
		const string kEntryAttrValueName = "value";

		void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			string[] names = kInfo.kValuesEnum.GetEnumNames();

			foreach (var element in s.ElementsByName(kEntryElementName))
				using (s.EnterCursorBookmark(element))
				{
					string name = null;
					s.ReadAttribute(kEntryAttrKeyName, ref name);

					int name_index = Array.IndexOf(names, name);
					if (name_index < 0)
						s.ThrowReadException(new System.IO.InvalidDataException("Invalid name value: " + name));

					mArray[name_index] = 0.0f;
					if (s.ReadAttributeOpt(kEntryAttrValueName, ref mArray[name_index])) // #HACK_BLAM: IgnoreWritePredicates hack! didn't used to be Opt
						mValidFlags |= 1U << name_index;
				}
		}
		void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			string[] names = kInfo.kValuesEnum.GetEnumNames();
			for (int x = 0, mask = 1; x < kInfo.kLength; x++, mask <<= 1)
			{
				bool has_value = (mValidFlags & (uint)mask) != 0;
				if (has_value)
				{
					using (s.EnterCursorBookmark(kEntryElementName))
					{
						s.WriteAttribute(kEntryAttrKeyName, names[x]);
						s.WriteAttribute(kEntryAttrValueName, mArray[x].ToString("r")); // round-trip for full float value
					}
				}
				else if (s.IgnoreWritePredicates) // #HACK_BLAM: IgnoreWritePredicates hack!
				{
					using (s.EnterCursorBookmark(kEntryElementName))
						s.WriteAttribute(kEntryAttrKeyName, names[x]);
				}
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
				 if (s.IsReading) Read(s);
			else if (s.IsWriting) Write(s);
		}
		#endregion
	};
}