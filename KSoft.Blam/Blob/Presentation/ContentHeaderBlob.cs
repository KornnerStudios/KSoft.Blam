using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Blob
{
	public sealed class ContentHeaderBlob
		: BlobObject
	{
		public const int kSizeOf = 0x2C0 - Blob.Transport.BlobChunkHeader.kSizeOf;

		short mBuildMajor;
		short mBuildMinor = TypeExtensions.kNone;
		public RuntimeData.ContentHeader Data { get; private set; }

		public bool IsFromMcc { get { return mBuildMajor == 0 && ((int)mBuildMinor).IsNone(); } }

		internal ContentHeaderBlob()
		{
			base.BlobFlags |= Transport.BlobChunkHeader.kFlagIsHeader;
		}

		public override int CalculateFixedBinarySize(Engine.BlamEngineTargetHandle gameTarget)
		{
			return kSizeOf;
		}

		protected override void InitializeExplicitlyForGame(Engine.BlamEngineTargetHandle gameTarget)
		{
			Data = RuntimeData.ContentHeader.Create(gameTarget.Build);

			Contract.Assert(gameTarget.Build.RevisionIndex.IsNotNone());
			mBuildMajor = (short)gameTarget.Build.Revision.Version;
		}

		public void ChangeData(RuntimeData.ContentHeader newData, int buildMajor = 0, int buildMinor = TypeExtensions.kNone)
		{
			Contract.Requires<ArgumentNullException>(newData != null);
			Contract.Requires<ArgumentException>(newData.GameBuild == Data.GameBuild);

			if (buildMajor != 0)
				mBuildMajor = (short)buildMajor;
			if (buildMinor.IsNotNone())
				mBuildMinor = (short)buildMinor;
			Data = newData;
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref mBuildMajor);
			s.Stream(ref mBuildMinor);

			bool stream_data_as_little_endian = false;
			if (s.IsReading)
			{
				if (IsFromMcc)
				{
					stream_data_as_little_endian = true;
				}
			}

			using (var endian_bm = s.BeginEndianSwitch(stream_data_as_little_endian ? Shell.EndianFormat.Little : s.ByteOrder))
			{
				s.Stream(Data);
			}
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			using (s.EnterCursorBookmark("ContentHeader"))
			{
				s.StreamAttribute("buildMajor", ref mBuildMajor);
				s.StreamAttribute("buildMinor", ref mBuildMinor);
				s.StreamObject(Data);
			}
		}
		#endregion
	};
}