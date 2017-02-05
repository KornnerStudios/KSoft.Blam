using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Blob.Transport
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public enum BlobChunkVerificationResult : short
	{
		/// <summary>The chunk header or one of its fields are in expected format</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.Data"/> is zero</remarks>
		Valid,
		/// <summary>The chunk was invalid in a non-uniform way</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.Data"/> may contain the invalid value</remarks>
		Invalid,

		#region Stream
		/// <summary>Not enough space in the BLF stream to perform the next stream operation</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsLength"/> will contain the amount of bytes left (long truncated to uint)</remarks>
		EndOfStream,
		/// <summary>The stream isn't open, reading or writing cannot take place</summary>
		StreamNotOpen,
		/// <summary>Stream is too small to hold the miminal amount of blob data (header, footer, and one chunk)</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsLength"/> will contain the stream length (long truncated to uint)</remarks>
		StreamTooSmall,
		/// <summary>The blob failed authentication</summary>
		AuthenticationFailed,
		#endregion
		#region Chunk
		/// <summary>The chunk's signature field did not match the expected value</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsSignature"/> will contain the invalid value</remarks>
		InvalidSignature,
		/// <summary>The chunk's total size did not match the expected value</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsSize"/> will contain the invalid value</remarks>
		InvalidSize,
		/// <summary>The chunk's version field did not match any expected values</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsVersion"/> will contain the invalid value</remarks>
		InvalidVersion,
		#endregion
		#region Header
		/// <summary>The BLF header's endian signature was an unexpected value</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.Data"/> will contain the invalid value</remarks>
		InvalidEndian,
		#endregion
		#region Footer
		/// <summary>The BLF footer's blob size didn't match what we've actually streamed</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsLength"/> will contain the invalid value</remarks>
		InvalidBlobSize,
		/// <summary>The BLF footer's authentication was an unexpected value</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsAuthentication"/> will contain the invalid value</remarks>
		InvalidAuthentication,
		/// <summary>The BLF footer's authentication data size was an unexpected value</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsSize"/> will contain the invalid value</remarks>
		InvalidAuthenticationSize,
		/// <summary>The BLF footer's authentication was valid, but not the expected kind</summary>
		/// <remarks><see cref="BlobChunkVerificationResultData.DataAsAuthentication"/> will contain the invalid value</remarks>
		AuthenticationMismatch,
		#endregion

	};
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum BlobChunkVerificationResultContext : short
	{
		Undefined,

		Stream,
		Chunk,
		Header,
		Footer,
	};
}