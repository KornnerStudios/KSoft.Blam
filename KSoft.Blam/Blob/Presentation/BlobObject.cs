using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Blob
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("Signature = {Description.Signature.TagString}, Version = {Version}")]
	public abstract class BlobObject
		: IO.IEndianStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public BlobGroup SystemGroup { get; private set; }
		public int Version { get; private set; }
		public int BlobFlags { get; internal set; }

		public abstract int CalculateFixedBinarySize(Engine.BlamEngineTargetHandle gameTarget);

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