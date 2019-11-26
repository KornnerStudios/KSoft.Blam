
namespace KSoft.Blam.Blob.Transport
{
	using AuthenticationTypeStreamer = IO.EnumBinaryStreamer<BlobTransportStreamAuthentication, byte>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public enum BlobTransportStreamAuthentication
	{
		None,
		Crc,
		Hash,
		Rsa,

		kMax,
	};

	partial class BlobTransportStream
	{
		struct StreamFooter
			: IO.IEndianStreamSerializable
		{
			const int kVersion = 1;
			internal const int kSizeOfDataSansAuthData = sizeof(uint) + sizeof(byte);

			internal static readonly Values.GroupTagData32 kSignature =
				new Values.GroupTagData32("_eof", "blob_footer");

			BlobChunkHeader Header;
			/// <summary>The size of the blob from the header up UNTIL the footer (so, exclusive)</summary>
			public uint BlobSize;
			public BlobTransportStreamAuthentication Authentication;
			public byte[] AuthenticationData;

			int InitializeData()
			{
				var data_size = Authentication.GetDataSize();
				if (data_size > 0)
					AuthenticationData = new byte[data_size];

				return data_size;
			}
			public StreamFooter(BlobTransportStreamAuthentication authentication, long blobSize)
			{
				BlobSize = (uint)blobSize;
				Authentication = authentication;
				AuthenticationData = null;

				Header = new BlobChunkHeader(kSignature, kVersion, kSizeOfDataSansAuthData + Authentication.GetDataSize());
				InitializeData();
			}

			int CalculateAssumedAuthenticationDataSize()
			{
				return Header.DataSize - kSizeOfDataSansAuthData;
			}
			public BlobChunkVerificationResultInfo Verify(BlobTransportStreamAuthentication expectedAuthentication,
				long blobSize)
			{
				var result = BlobChunkVerificationResultInfo.ValidResult;

				if (BlobSize != blobSize)
				{
					result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidBlobSize,
						BlobChunkVerificationResultContext.Footer, BlobSize);
				}
				else if (Authentication < 0 || Authentication >= BlobTransportStreamAuthentication.kMax)
				{
					result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidAuthentication,
						BlobChunkVerificationResultContext.Footer, Authentication);
				}
				else if (Authentication != expectedAuthentication)
				{
					result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.AuthenticationMismatch,
						BlobChunkVerificationResultContext.Footer, Authentication);
				}
				else if (CalculateAssumedAuthenticationDataSize() != Authentication.GetDataSize())
				{
					result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidAuthenticationSize,
						BlobChunkVerificationResultContext.Footer, CalculateAssumedAuthenticationDataSize());
				}

				return result;
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Header);
				s.Stream(ref BlobSize);
				s.Stream(ref Authentication, AuthenticationTypeStreamer.Instance);
			}
			public void SerializeSansHeader(IO.EndianStream s, BlobChunkHeader providedHeader)
			{
				Header = providedHeader;
				s.Stream(ref BlobSize);
				s.Stream(ref Authentication, AuthenticationTypeStreamer.Instance);
			}
			public void SerializeAuthenticationData(IO.EndianStream s)
			{
				if (s.IsReading)
					InitializeData();

				if (Authentication != BlobTransportStreamAuthentication.None)
					s.Stream(AuthenticationData);
			}
			#endregion
		};
	};
}