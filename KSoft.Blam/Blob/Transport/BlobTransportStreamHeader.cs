using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Blob.Transport
{
	partial class BlobTransportStream
	{
		struct StreamHeader
			: IO.IEndianStreamSerializable
		{
			const int kVersion = 1;
			const int kFlags = BlobChunkHeader.kFlagIsHeader;
			internal const int kSizeOfData = sizeof(short) + (TypeExtensionsBlam.kTagStringLength+1) +
				sizeof(short); // padding

			internal static readonly Values.GroupTagData32 kSignature =
				new Values.GroupTagData32("_blf", "blob_header");
			static readonly BlobChunkHeader kChunkSignature =
				new BlobChunkHeader(kSignature, kVersion, kSizeOfData, kFlags);
			const short kEndianSignature = -2;

			BlobChunkHeader Header;
			short EndianSignature;
			public string FileType;

			public StreamHeader(string fileType)
			{
				Header = kChunkSignature;
				EndianSignature = kEndianSignature;
				FileType = fileType;
			}

			#region Verify
			BlobChunkVerificationResultInfo VerifyEndian(out bool requiresByteswap)
			{
				requiresByteswap = false;
				var result = BlobChunkVerificationResultInfo.ValidResult;

				if (EndianSignature != kEndianSignature)
				{
					requiresByteswap = Bitwise.ByteSwap.SwapInt16(EndianSignature) == kEndianSignature;

					if (!requiresByteswap) // the signature didn't match, even after byte swapping it
						result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.InvalidEndian,
							BlobChunkVerificationResultContext.Header, EndianSignature);
				}

				return result;
			}
			public BlobChunkVerificationResultInfo Verify(out bool requiresByteswap)
			{
				var result = VerifyEndian(out requiresByteswap);

				return result	.And(Header, c => c.VerifySignature(kChunkSignature.Signature))
								.And(Header, c => c.VerifyVersion(kChunkSignature.Version))
								.And(Header, c => c.VerifyDataSize(kSizeOfData));
			}
			#endregion

			internal void ByteSwap()
			{
				Header.ByteSwap();
				Bitwise.ByteSwap.Swap(ref EndianSignature);
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Header);
				s.Stream(ref EndianSignature);
				s.Stream(ref FileType, TypeExtensionsBlam.kTagStringEncoding);
				s.Pad16();
			}
			#endregion
		};
	};
}