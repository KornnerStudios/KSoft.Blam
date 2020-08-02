using System;
using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Blam.Blob.Transport
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("{Context}, {Result}, Data = {Data}")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct BlobChunkVerificationResultInfo
	{
		public static readonly BlobChunkVerificationResultInfo ValidResult =
			new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.Valid, BlobChunkVerificationResultContext.Undefined);

		BlobChunkVerificationResult mResult;
		BlobChunkVerificationResultContext mContext;
		uint mData;

		public BlobChunkVerificationResult Result { get { return mResult; } }
		public BlobChunkVerificationResultContext Context
		{
			get { return mContext; }
			internal set { mContext = value; }
		}
		#region Data-As util
		public uint Data			{ get { return mData; } }
		public uint DataAsSignature	{ get { return mData; } }
		public int DataAsSize		{ get { return (int)mData; } }
		public int DataAsVersion	{ get { return (int)mData; } }
		public uint DataAsLength	{ get { return mData; } }
		public BlobTransportStreamAuthentication DataAsAuthentication { get { return (BlobTransportStreamAuthentication)mData; } }
		#endregion

		public bool IsValid		{ get { return Result == BlobChunkVerificationResult.Valid; } }
		public bool IsInvalid	{ get { return Result != BlobChunkVerificationResult.Valid; } }

		#region Ctor
		internal BlobChunkVerificationResultInfo(BlobChunkVerificationResult result, BlobChunkVerificationResultContext context,
			uint data = 0)
		{
			mResult = result;
			mContext = context;
			mData = data;
		}
		internal BlobChunkVerificationResultInfo(BlobChunkVerificationResult result, BlobChunkVerificationResultContext context,
			int data)
		{
			mResult = result;
			mContext = context;
			mData = (uint)data;
		}
		internal BlobChunkVerificationResultInfo(BlobChunkVerificationResult result, BlobChunkVerificationResultContext context,
			long data)
		{
			mResult = result;
			mContext = context;
			mData = (uint)data;
		}
		internal BlobChunkVerificationResultInfo(BlobChunkVerificationResult result, BlobChunkVerificationResultContext context,
			BlobTransportStreamAuthentication data)
		{
			mResult = result;
			mContext = context;
			mData = (uint)data;
		}
		#endregion

		#region Logical 'And' util
		[Contracts.Pure]
		public BlobChunkVerificationResultInfo And<T>(T contextObj,
			Func<T, BlobChunkVerificationResultInfo> lhs)
		{
			if (IsValid)
				return lhs(contextObj);

			return this;
		}
		[Contracts.Pure]
		public BlobChunkVerificationResultInfo And<T, TParam>(T contextObj, TParam param,
			Func<T, TParam, BlobChunkVerificationResultInfo> lhs)
		{
			if (IsValid)
				return lhs(contextObj, param);

			return this;
		}
		#endregion

		public string BuildErrorMessage()
		{
			if (IsValid)
				return "No error";

			var sb = new System.Text.StringBuilder();

			string ctxt_string = Context.ToString();
			switch (Context)
			{
				case BlobChunkVerificationResultContext.Chunk:
					ctxt_string = "a " + ctxt_string;
					break;
				case BlobChunkVerificationResultContext.Stream:
				case BlobChunkVerificationResultContext.Header:
				case BlobChunkVerificationResultContext.Footer:
					ctxt_string = "the " + ctxt_string;
					break;

				default: break;
			}

			switch (Result)
			{
				case BlobChunkVerificationResult.Invalid:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Unknown error ({0})", Data.ToString("X8", Util.InvariantCultureInfo));
					break;

				#region Stream
				case BlobChunkVerificationResult.EndOfStream:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Encountered EOF during a read in {0}. Only had {1} bytes left",
						ctxt_string, DataAsLength);
					break;
				case BlobChunkVerificationResult.StreamNotOpen:
					sb.Append("Tried to operate on a BLF stream without opening it first");
					break;
				case BlobChunkVerificationResult.StreamTooSmall:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Stream is too small to be a BLF source ({0} bytes)", DataAsLength);
					break;
				case BlobChunkVerificationResult.AuthenticationFailed:
					sb.Append("Failed to authenticate the BLF stream");
					break;
				#endregion
				#region Chunk
				case BlobChunkVerificationResult.InvalidSignature:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Encountered an invalid chunk signature '{0}' while reading {1}",
						DataAsSignature, ctxt_string);
					break;
				case BlobChunkVerificationResult.InvalidSize:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Encountered an invalid chunk size '{0}' while reading {1}",
						DataAsSize, ctxt_string);
					break;
				case BlobChunkVerificationResult.InvalidVersion:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Encountered an invalid chunk version '{0}' while reading {1}",
						DataAsVersion, ctxt_string);
					break;
				#endregion
				#region Header
				case BlobChunkVerificationResult.InvalidEndian:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"BLF header endian bytes is invalid '{0}'", Data.ToString("X4", Util.InvariantCultureInfo));
					break;
				#endregion
				#region Footer
				case BlobChunkVerificationResult.InvalidBlobSize:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Footer's blob size didn't match actual blob stream length", DataAsLength);
					break;
				case BlobChunkVerificationResult.InvalidAuthentication:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Footer specified an invalid authentication '{0}'", DataAsAuthentication);
					break;
				case BlobChunkVerificationResult.InvalidAuthenticationSize:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Footer's authentication size is invalid '{0}'", DataAsSize);
					break;
				case BlobChunkVerificationResult.AuthenticationMismatch:
					sb.AppendFormat(Util.InvariantCultureInfo,
						"Footer specified an unexpected authentication '{0}'", DataAsAuthentication);
					break;
				#endregion

				default:
					sb.Append("Corrupt result value, yell for a programmer");
					break;
			}

			return sb.ToString();
		}
	};
}
