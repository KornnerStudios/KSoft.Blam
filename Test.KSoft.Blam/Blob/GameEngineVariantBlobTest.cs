using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Blob.Test
{
	[TestClass]
	public sealed class GameEngineVariantBlobTest : BaseTestClass
	{
		[TestMethod]
		public void Serialize_WritesFixedHashBufferAndRestoresStreamPosition()
		{
			const int k_hash_size = 0x14;
			const int k_padding_size = sizeof(uint);
			const int k_bitstream_length_size = sizeof(int);
			byte[] prefix = [0xA5, 0x5A, 0xC3, 0x3C, 0x96, 0x69, 0xF0];

			Engine.EngineBuildHandle game_build =
				Engine.EngineRegistry.EngineBranchHaloReach.Revisions[0].BuildHandle;
			Engine.BlamEngineTargetHandle game_target = game_build.ToEngineTargetHandle();

			using var blob_system_ref = Engine.EngineRegistry.GetSystem<BlobSystem>(game_build);
			var blob = (GameEngineVariantBlob)blob_system_ref.System.CreateObject(
				game_target, WellKnownBlob.GameVariant);
			using var blob_data = blob.Data!;
			using var memory_stream = new MemoryStream();
			memory_stream.Write(prefix);

			using (var endian_stream = new IO.EndianStream(memory_stream,
				Shell.EndianFormat.Big, name: nameof(GameEngineVariantBlobTest),
				permissions: FileAccess.Write))
			{
				endian_stream.BaseStreamOwner = false;
				endian_stream.StreamMode = FileAccess.Write;

				blob.Serialize(endian_stream);

				long expected_stream_position =
					prefix.Length + blob.CalculateFixedBinarySize(game_target);
				Assert.AreEqual(expected_stream_position, memory_stream.Position);
			}

			byte[] serialized = memory_stream.ToArray();
			int hash_offset = prefix.Length;
			int padding_offset = hash_offset + k_hash_size;
			int bitstream_length_offset = padding_offset + k_padding_size;
			int bitstream_offset = bitstream_length_offset + k_bitstream_length_size;
			int bitstream_capacity = serialized.Length - bitstream_offset;
			int bitstream_length = BinaryPrimitives.ReadInt32BigEndian(
				serialized.AsSpan(bitstream_length_offset, k_bitstream_length_size));

			Assert.AreEqual(
				prefix.Length + blob.CalculateFixedBinarySize(game_target),
				serialized.Length);
			CollectionAssert.AreEqual(prefix, serialized.AsSpan(0, prefix.Length).ToArray());
			CollectionAssert.AreEqual(
				new byte[k_padding_size],
				serialized.AsSpan(padding_offset, k_padding_size).ToArray());
			Assert.IsTrue(bitstream_length > 0 && bitstream_length <= bitstream_capacity);
			Assert.IsTrue(serialized
				.AsSpan(bitstream_offset + bitstream_length, bitstream_capacity - bitstream_length)
				.ToArray()
				.All(value => value == 0));

			byte[] expected_hash;
			using (var hasher = Program.GetGen3RuntimeDataHasher())
			{
				hasher.TransformBlock(serialized,
					bitstream_length_offset, k_bitstream_length_size, null, 0);
				hasher.TransformFinalBlock(serialized, bitstream_offset, bitstream_length);
				expected_hash = hasher.Hash!;
			}

			CollectionAssert.AreEqual(
				expected_hash,
				serialized.AsSpan(hash_offset, k_hash_size).ToArray());
		}
	}
}
