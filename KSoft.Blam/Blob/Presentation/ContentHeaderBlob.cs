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

		ushort mBuildNumber;
		ushort mFlags;
		public RuntimeData.ContentHeader Data { get; private set; }

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
			mBuildNumber = (ushort)gameTarget.Build.Revision.Version;
		}

		public void ChangeData(RuntimeData.ContentHeader newData, uint buildNumber = 0, uint flags = 0)
		{
			Contract.Requires<ArgumentNullException>(newData != null);
			Contract.Requires<ArgumentException>(newData.GameBuild == Data.GameBuild);

			if (buildNumber != 0)
				mBuildNumber = (ushort)buildNumber;
			mFlags = (ushort)flags;
			Data = newData;
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref mBuildNumber);
			s.Stream(ref mFlags);
			s.Stream(Data);
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
				s.StreamObject(Data);
			}
		}
		#endregion
	};
}