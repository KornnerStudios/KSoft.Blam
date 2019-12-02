#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Localization.StringTables
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class LocaleStringTableInfo
	{
		const int kDefaultBufferCompressionThreshold = 0x80;

		public readonly int MaxCount;
		internal readonly int CountBitLength;

		public readonly int BufferMaxSize;
		internal int BufferSizeBitLength;
		internal int BufferOffsetBitLength;
		internal int BufferCompressionThreshold; // if the buffer is less than this, don't compress

		internal readonly bool CodeNameEntries;

		#region Ctor
		internal LocaleStringTableInfo(int maxCount, int bufferMaxSize, bool codeNameEntries = false)
		{
			MaxCount = maxCount;
			CountBitLength = Bits.GetMaxEnumBits(maxCount + 1);

			BufferMaxSize = bufferMaxSize;
			BufferSizeBitLength = Bits.GetMaxEnumBits(bufferMaxSize + 1);
			BufferOffsetBitLength = Bits.GetMaxEnumBits(bufferMaxSize);
			BufferCompressionThreshold = kDefaultBufferCompressionThreshold;

			CodeNameEntries = codeNameEntries;
		}
		internal LocaleStringTableInfo SetBufferRelatedBitLengths(int bufferOffsetBitLength, int bufferSizeBitLength)
		{
			Contract.Requires(bufferOffsetBitLength <= Bits.kInt32BitCount);
			Contract.Requires(bufferSizeBitLength <= Bits.kInt32BitCount);
			Contract.Requires(bufferOffsetBitLength <= bufferSizeBitLength);

			BufferSizeBitLength = bufferSizeBitLength;
			BufferOffsetBitLength = bufferOffsetBitLength;

			return this;
		}
		#endregion
	};
}