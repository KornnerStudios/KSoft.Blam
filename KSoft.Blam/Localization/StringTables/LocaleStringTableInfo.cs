using System;

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
			if (bufferOffsetBitLength > Bits.kInt32BitCount)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferOffsetBitLength), bufferOffsetBitLength,
					string.Format(Util.InvariantCultureInfo,
						"Buffer offset bit length must be at most {0}.", Bits.kInt32BitCount));
			}
			if (bufferSizeBitLength > Bits.kInt32BitCount)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferSizeBitLength), bufferSizeBitLength,
					string.Format(Util.InvariantCultureInfo,
						"Buffer size bit length must be at most {0}.", Bits.kInt32BitCount));
			}
			if (bufferOffsetBitLength > bufferSizeBitLength)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Buffer offset bit length must be <= size bit length; offset {0}, size {1}.",
					bufferOffsetBitLength, bufferSizeBitLength), nameof(bufferOffsetBitLength));
			}

			BufferSizeBitLength = bufferSizeBitLength;
			BufferOffsetBitLength = bufferOffsetBitLength;

			return this;
		}
		#endregion
	};
}