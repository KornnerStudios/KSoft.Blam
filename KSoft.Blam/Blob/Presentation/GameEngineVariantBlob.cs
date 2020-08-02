using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Blob
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameEngineVariantBlob
		: BlobObject
	{
		public static bool RequireValidHashes { get; set; } = true;

		#region Constants
		const int kSizeOfBitStreamHaloReach = 0x5000;
		const int kSizeOfBitStreamHalo4 = 0x7C00;
		const int kSizeOfHash = 0x14;
		const int kSizeOfPrememble = kSizeOfHash +
			sizeof(uint) + // unused/uninitialized (should only ever contain garbage/stack data)
			sizeof(int); // bitstream length

		const int kSizeOfHaloReach = kSizeOfPrememble + kSizeOfBitStreamHaloReach;
		const int kSizeOfHalo4 = kSizeOfPrememble + kSizeOfBitStreamHalo4;
		#endregion

		public bool InvalidData { get; private set; }

		public RuntimeData.Variants.GameEngineVariant Data { get; private set; }

		public override int CalculateFixedBinarySize(Engine.BlamEngineTargetHandle gameTarget)
		{
			var game_build = gameTarget.Build;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return kSizeOfHaloReach;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
				return kSizeOfHalo4;

			throw new KSoft.Debug.UnreachableException(game_build.ToDisplayString());
		}

		protected override void InitializeExplicitlyForGame(Engine.BlamEngineTargetHandle gameTarget)
		{
			Data = new RuntimeData.Variants.GameEngineVariant(gameTarget.Build);
		}

		public void ChangeData(RuntimeData.Variants.GameEngineVariant newData)
		{
			Contract.Requires<ArgumentNullException>(newData != null);
			Contract.Requires<ArgumentException>(newData.GameBuild == GameTarget.Build);

			Data = newData;
		}

		void SanityCheckInvalidIsFalse(string streamName)
		{
			if (InvalidData)
			{
				if (RequireValidHashes)
				{
					throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"{0}: game variant bitstream is invalid, can't operate", streamName));
				}
				else
				{
					Debug.Trace.Blob.TraceDataSansId(System.Diagnostics.TraceEventType.Warning,
						"Blob had mismatching hash",
						this);
				}
			}
		}

		#region IEndianStreamSerializable Members
		int GetBitStreamSize()
		{
			var game_build = GameTarget.Build;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return kSizeOfBitStreamHaloReach;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
				return kSizeOfBitStreamHalo4;

			throw new KSoft.Debug.UnreachableException(game_build.ToDisplayString());
		}
		static int ReadBitStreamSize(IO.EndianReader s, System.Security.Cryptography.ICryptoTransform hasher,
			int maxBitStreamSize, bool isProbablyFromMcc)
		{
			byte[] bitstream_size_bytes = new byte[sizeof(int)];
			s.Read(bitstream_size_bytes);
			hasher.TransformBlock(bitstream_size_bytes, 0, bitstream_size_bytes.Length, null, 0);

			if (!s.ByteOrder.IsSameAsRuntime())
				Bitwise.ByteSwap.SwapData(Bitwise.ByteSwap.kInt32Definition, bitstream_size_bytes);
			int assumed_size = BitConverter.ToInt32(bitstream_size_bytes, 0);
			int size = assumed_size;

			bool size_was_in_range_after_forced_byte_swap = false;
			if (isProbablyFromMcc)
			{
				size = Bitwise.ByteSwap.SwapInt32(size);
			}
			else if (assumed_size < 0 || assumed_size > maxBitStreamSize)
			{
				size = Bitwise.ByteSwap.SwapInt32(size);
				if (size <= maxBitStreamSize)
				{
					size_was_in_range_after_forced_byte_swap = true;
				}
				else
				{
					throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"Invalid bitstream size {0}, data is probably corrupt. Max size is {1}",
						assumed_size.ToString("X8", Util.InvariantCultureInfo),
						maxBitStreamSize.ToString("X8", Util.InvariantCultureInfo)));
				}
			}

			Util.MarkUnusedVariable(ref size_was_in_range_after_forced_byte_swap);

			return size;
		}
		void ReadBitStream(IO.EndianReader s, byte[] hashBuffer)
		{
			int max_bit_stream_size = GetBitStreamSize();
			bool is_probably_from_mcc = hashBuffer.EqualsZero();

			byte[] bs_bytes;
			using (var hasher = Program.GetGen3RuntimeDataHasher())
			{
				int bs_length = ReadBitStreamSize(s, hasher, max_bit_stream_size, is_probably_from_mcc);
				bs_bytes = new byte[IntegerMath.Align(IntegerMath.kInt32AlignmentBit, bs_length)];
				s.Read(bs_bytes, bs_length);

				hasher.TransformFinalBlock(bs_bytes, 0, bs_length);
				InvalidData = hasher.Hash.EqualsArray(hashBuffer) == false;
			}

			if (RequireValidHashes && InvalidData)
			{
				Data = null;
			}
			else
			{
				using (var ms = new System.IO.MemoryStream(bs_bytes))
				using (var bs = new IO.BitStream(ms, System.IO.FileAccess.Read, streamName: "GameVariant"))
				{
					bs.StreamMode = System.IO.FileAccess.Read;

					Data.Serialize(bs);
				}
			}
		}
		void WriteBitStream(IO.EndianWriter s, long hashPosition)
		{
			byte[] bs_bytes = new byte[GetBitStreamSize()];
			int bs_length;
			using (var ms = new System.IO.MemoryStream(bs_bytes))
			using (var bs = new IO.BitStream(ms, System.IO.FileAccess.Write, streamName: "GameVariant"))
			{
				bs.StreamMode = System.IO.FileAccess.Write;

				Data.Serialize(bs);
				bs.Flush();

				bs_length = (int)ms.Position;
			}

			#region calculate hash
			byte[] calculated_hash;
			using (var hasher = Program.GetGen3RuntimeDataHasher())
			{
				byte[] bs_length_bytes = BitConverter.GetBytes(bs_length);
				if (!s.ByteOrder.IsSameAsRuntime())
					Bitwise.ByteSwap.SwapData(Bitwise.ByteSwap.kInt32Definition, bs_length_bytes);

				hasher.TransformBlock(bs_length_bytes, 0, bs_length_bytes.Length, null, 0);
				hasher.TransformFinalBlock(bs_bytes, 0, bs_length);
				calculated_hash = hasher.Hash;
			}
			#endregion

			s.Write(bs_length);
			s.Write(bs_bytes);

			long position = s.BaseStream.Position;
			s.Seek(hashPosition);
			s.Write(calculated_hash);
			s.Seek(position);
		}
		public override void Serialize(IO.EndianStream s)
		{
			SanityCheckInvalidIsFalse(s.StreamName);

			byte[] hash_buffer = new byte[kSizeOfHash];

			long hash_position = s.BaseStream.Position;
			s.Stream(hash_buffer);
			s.Pad32();

				 if (s.IsReading)	ReadBitStream(s.Reader, hash_buffer);
			else if (s.IsWriting)	WriteBitStream(s.Writer, hash_position);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			SanityCheckInvalidIsFalse(s.StreamName);
			base.Serialize(s);

			using (s.EnterCursorBookmark("GameVariant"))
			{
				s.StreamObject(Data);
			}
		}
		#endregion

		public static long GetBlfFileLength(Engine.BlamEngineTargetHandle gameTarget)
		{
			var game_build = gameTarget.Build;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return kSizeOfBitStreamHaloReach + 0x329;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
				return kSizeOfBitStreamHalo4 + 0x329;

			throw new KSoft.Debug.UnreachableException(game_build.ToDisplayString());
		}
	};
}
