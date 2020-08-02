
namespace KSoft.Blam.Blob
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("Signature = {Description.Signature.TagString}, Version = {Version}")]
	public abstract class BlobObject
		: IO.IEndianStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public BlobGroup SystemGroup { get; private set; }
		public BlobGroupVersionAndBuildInfo SystemGroupVersionInfo { get; private set; }
		public Engine.BlamEngineTargetHandle GameTarget { get; private set; }
		public int Version { get; private set; }
		public int BlobFlags { get; internal set; }

		public abstract int CalculateFixedBinarySize(Engine.BlamEngineTargetHandle gameTarget);

		#region internal Initialize
		internal void Initialize(BlobSystem system, Engine.BlamEngineTargetHandle gameTarget,
			BlobGroup blobGroup, int version)
		{
			Util.MarkUnusedVariable(ref system);

			SystemGroup = blobGroup;
			GameTarget = gameTarget;
			Version = version;

			BlobGroupVersionAndBuildInfo info_for_version;
			if (SystemGroup.VersionAndBuildMap.TryGetValue(Version, out info_for_version))
			{
				SystemGroupVersionInfo = info_for_version;
			}
			else
			{
				throw new KSoft.Debug.UnreachableException();
			}

			InitializeExplicitlyForGame(gameTarget);
		}
		protected virtual void InitializeExplicitlyForGame(Engine.BlamEngineTargetHandle gameTarget)
		{
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
		}
		#endregion
	};
}
