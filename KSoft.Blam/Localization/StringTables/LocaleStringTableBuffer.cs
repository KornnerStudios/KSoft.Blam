
namespace KSoft.Blam.Localization.StringTables
{
	[System.Reflection.Obfuscation(Exclude=false)]
	internal class LocaleStringTableBuffer
		: IO.IBitStreamSerializable
	{
		LocaleStringTable mOwner;
		public byte[] Buffer;

		public LocaleStringTableBuffer(LocaleStringTable owner)
		{
			mOwner = owner;
		}

		#region IBitStreamSerializable Members
		void Read(IO.BitStream bs)
		{
			int uncompressed_size;
			bool is_compressed;

			bs.Read(out uncompressed_size, mOwner.kInfo.BufferSizeBitLength); int size = uncompressed_size;
			if (uncompressed_size > mOwner.kInfo.BufferMaxSize)
				throw new System.IO.InvalidDataException("Input string table buffer size too large by (bytes): " +
					(uncompressed_size - mOwner.kInfo.BufferMaxSize).ToString());

			bs.Read(out is_compressed);
			if (is_compressed)
				bs.Read(out size, mOwner.kInfo.BufferSizeBitLength);

			Buffer = new byte[size];
			bs.Read(Buffer);

			if (is_compressed)
				Buffer = IO.Compression.ZLib.LowLevelDecompress(Buffer, uncompressed_size);
		}
		void Write(IO.BitStream bs)
		{
			byte[] buffer = Buffer;
			bool is_compressed = buffer.Length > mOwner.kInfo.BufferCompressionThreshold;

			bs.Write(buffer.Length, mOwner.kInfo.BufferSizeBitLength);
			bs.Write(is_compressed);
			if (is_compressed)
			{
				buffer = IO.Compression.ZLib.LowLevelCompress(buffer, Shell.EndianFormat.Big);
				bs.Write(buffer.Length, mOwner.kInfo.BufferSizeBitLength);
			}

			bs.Write(buffer);
		}
		public void Serialize(IO.BitStream s)
		{
				 if (s.IsReading) Read(s);
			else if (s.IsWriting) Write(s);
		}
		#endregion
	};
}