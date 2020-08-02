#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct ListLimitTraits
		: IO.ITagElementStringNameStreamable
	{
		public static readonly ListLimitTraits Null = new ListLimitTraits
		{ MaxCount=0, CountBitLength=-1, IndexBitLength=-1 };

		public int MaxCount;
		internal int CountBitLength;
		internal int IndexBitLength;

		public void InitializeBitLengths()
		{
			Contract.Assume(MaxCount > 0);
			CountBitLength = Bits.GetMaxEnumBits(MaxCount + 1);
			IndexBitLength = Bits.GetMaxEnumBits(MaxCount);
		}

		internal int GetBitLength(bool useIndex)
		{
			return useIndex ? IndexBitLength : CountBitLength;
		}

		internal void ValidateListCount(System.Collections.IList list, string listName
			, IO.ICanThrowReadExceptionsWithExtraDetails readExceptionThrower)
		{
			if (list.Count > MaxCount)
			{
				var ex = new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0} exceeded its maximum number of elements; {1} > {2}", listName, list.Count, MaxCount));

				readExceptionThrower.ThrowReadExeception(ex);
			}
		}

		#region ITagElementStringNameStreamable Members
		internal static void SerializeViaElement<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string elementName, ref ListLimitTraits traits)
			where TDoc : class
			where TCursor : class
		{
			s.StreamElement(elementName, ref traits.MaxCount);

			if (s.IsReading)
				traits.InitializeBitLengths();
		}
		internal static void SerializeViaElementOpt<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string elementName, ref ListLimitTraits traits)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = s.StreamElementOpt(elementName, ref traits.MaxCount, Predicates.IsNotZero);

			if (streamed && s.IsReading)
				traits.InitializeBitLengths();
		}
		internal static void SerializeViaAttribute<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref ListLimitTraits traits)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute(attributeName, ref traits.MaxCount);

			if (s.IsReading)
				traits.InitializeBitLengths();
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("max", ref MaxCount);

			if (s.IsReading)
				InitializeBitLengths();
		}
		#endregion
	};
}
