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
			new(BlobChunkVerificationResult.Valid, BlobChunkVerificationResultContext.Undefined);

		readonly BlobChunkVerificationResult mResult;
		BlobChunkVerificationResultContext mContext;
		readonly uint mData;

		public readonly BlobChunkVerificationResult Result => mResult;
		public BlobChunkVerificationResultContext Context
		{
			readonly get { return mContext; }
			internal set { mContext = value; }
		}
		#region Data-As util
		public readonly uint Data => mData;
		public readonly uint DataAsSignature => mData;
		public readonly int DataAsSize => (int)mData;
		public readonly int DataAsVersion => (int)mData;
		public readonly uint DataAsLength => mData;
		public readonly BlobTransportStreamAuthentication DataAsAuthentication => (BlobTransportStreamAuthentication)mData;
		#endregion

		public readonly bool IsValid =>		Result == BlobChunkVerificationResult.Valid;
		public readonly bool IsInvalid =>	Result != BlobChunkVerificationResult.Valid;

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
		public readonly BlobChunkVerificationResultInfo And<T>(T contextObj,
			Func<T, BlobChunkVerificationResultInfo> lhs)
		{
			if (IsValid)
			{
				return lhs(contextObj);
			}

			return this;
		}
		[Contracts.Pure]
		public readonly BlobChunkVerificationResultInfo And<T, TParam>(T contextObj, TParam param,
			Func<T, TParam, BlobChunkVerificationResultInfo> lhs)
		{
			if (IsValid)
			{
				return lhs(contextObj, param);
			}

			return this;
		}
		#endregion

		public readonly string BuildErrorMessage()
		{
			if (IsValid)
			{
				return "No error";
			}

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
						"Footer's blob size '{0}' didn't match actual blob stream length", DataAsLength);
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
