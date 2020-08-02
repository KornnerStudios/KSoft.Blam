
namespace KSoft.Blam.Blob.Transport
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("{Signature}, {DataSize}, {Version}, {Flags}")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct BlobChunkHeader
		: IO.IEndianStreamSerializable
	{
		#region Constants
		public const int kFlagIsInitialized = 1<<0; // any blob who is any blob has this set. Never seen it not.
		public const int kFlagIsHeader = 1<<1; // both _blf and chdr have this set

		public const int kSizeOf = 0xC;

		internal static readonly BlobChunkHeader Null = new BlobChunkHeader()
		{
			Signature = 0,
			Size = kSizeOf,
			Version = 0,
			Flags = 0,
		};
		#endregion

		/// <summary>The four character code signature of the chunk</summary>
		public uint Signature;
		/// <summary>Size of the ENTIRE chunk (including the header)</summary>
		internal int Size;
		/// <summary>The version id of the chunk data</summary>
		public short Version;
		// First I thought this was an alignment bit, but EOF's usage doesn't suggest this (unless it's an alignment 'byte')
		// Also, I can't find any code that actually makes use of it at runtime
		public short Flags;

		/// <summary>Size of the chunk's data (ie, exluding the header)</summary>
		public int DataSize
		{
			get { return Size - kSizeOf; }
			set { Size = value + kSizeOf; }
		}

		public BlobChunkHeader(Values.GroupTagData32 signature, int version,
			int dataSize = TypeExtensions.kNone, int flags = kFlagIsInitialized)
		{
			Signature = signature.ID;
			Size = kSizeOf;
			if (dataSize.IsNotNone())
				Size += dataSize;
			Version = (short)version;
			Flags = (short)flags;
		}

		#region Verify
		/// <summary>Checks that there's enough chunk to store both this header and the <paramref name="expectedDataSize"/></summary>
		/// <param name="expectedDataSize">The minimum amount of bytes needed for the data (exlusive of header data)</param>
		/// <returns></returns>
		internal BlobChunkVerificationResultInfo VerifyDataSize(int expectedDataSize)
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			if (Size < (expectedDataSize + kSizeOf))
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidSize,
					BlobChunkVerificationResultContext.Chunk, Size);

			return result;
		}
		internal BlobChunkVerificationResultInfo VerifySignature(uint expectedSignature)
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			if (Signature != expectedSignature)
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidSignature,
					BlobChunkVerificationResultContext.Chunk, Signature);

			return result;
		}
		internal BlobChunkVerificationResultInfo VerifyVersion(int expectedVersion)
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			if (Version != expectedVersion)
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidVersion,
					BlobChunkVerificationResultContext.Chunk, Version);

			return result;
		}
		internal BlobChunkVerificationResultInfo VerifyVersionIsPositive()
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			if (Version < 0)
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidVersion,
					BlobChunkVerificationResultContext.Chunk, Version);

			return result;
		}
		internal BlobChunkVerificationResultInfo VerifyAnyVersion(params int[] expectedVersions)
		{
			var result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidVersion,
					BlobChunkVerificationResultContext.Chunk, Version);

			foreach (var expected_version in expectedVersions)
			{
				if (Version != expected_version)
					continue;

				result = BlobChunkVerificationResultInfo.ValidResult;
				break;
			}

			return result;
		}
		// Neither the header or footer even check touch the flags value
		internal BlobChunkVerificationResultInfo VerifyFlagsIsPostive()
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			if ((int)Flags < 0)
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.Invalid,
					BlobChunkVerificationResultContext.Chunk, (uint)Flags);

			return result;
		}
		internal BlobChunkVerificationResultInfo Verify(uint expectedSignature, int expectedSize, int expectedVersion)
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			return result	.And(this, expectedSignature,	(c, param) => c.VerifySignature(param))
							.And(this, expectedVersion,		(c, param) => c.VerifyVersion(param))
							.And(this, expectedSize,		(c, param) => c.VerifyDataSize(param))
							.And(this,						c => c.VerifyFlagsIsPostive());
		}
		#endregion

		internal void ByteSwap()
		{
			Bitwise.ByteSwap.Swap(ref Signature);
			Bitwise.ByteSwap.Swap(ref Size);
			Bitwise.ByteSwap.Swap(ref Version);
			Bitwise.ByteSwap.Swap(ref Flags);
		}

		#region IEndianStreamSerializable Members
		internal void StreamSkipData(System.IO.Stream stream)
		{
			stream.Seek(DataSize, System.IO.SeekOrigin.Current);
		}

		public void Serialize(IO.EndianStream s)
		{
			s.StreamTag(ref Signature);
			s.Stream(ref Size);
			s.Stream(ref Version);
			s.Stream(ref Flags);
		}
		#endregion
	};
}
