using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Blob
{
	public sealed class ContentHeaderBlob : BlobObject
	{
		public const int kSizeOf = 0x2C0 - Blob.Transport.BlobChunkHeader.kSizeOf;

		ushort mBuildNumber;
		ushort mFlags;
		//public ContentHeader Data { get; private set; }
		
		internal ContentHeaderBlob()
		{
			base.BlobFlags |= Transport.BlobChunkHeader.kFlagIsHeader;
		}

		public override int CalculateFixedBinarySize(Engine.BlamEngineTargetHandle gameTarget)
		{
			return kSizeOf;
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref mBuildNumber);
			s.Stream(ref mFlags);
			Contract.Assert(false);
			//s.Stream(Data);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			using (s.EnterCursorBookmark("ContentHeader"))
			{
				s.StreamAttribute("buildNumber", ref mBuildNumber);
				s.StreamAttribute("flags", ref mFlags);
				Contract.Assert(false);
				//s.StreamObject(Data);
			}
		}
		#endregion
	};
}