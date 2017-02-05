using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.RuntimeData.Variants
{
	using MapPermissionsExceptionTypeBitStreamer = IO.EnumBitStreamer<MapPermissionsExceptionType>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MapPermissionsExceptionType : byte
	{
		Inclusive, // Can be played on these maps only
		Exclusive, // Can't be played on these maps
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloVariantMapPermissions
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public List<int> MapIds { get; private set; }
		public MapPermissionsExceptionType ExceptionType;

		public bool IsDefault { get {
			return ExceptionType == MapPermissionsExceptionType.Exclusive && MapIds.Count == 0;
		} }

		public MegaloVariantMapPermissions()
		{
			MapIds = new List<int>();
			ExceptionType = MapPermissionsExceptionType.Exclusive;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.StreamElements(MapIds, 6, Bits.kInt16BitCount);
			s.Stream(ref ExceptionType, 1, MapPermissionsExceptionTypeBitStreamer.Instance);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("exceptionType", ref ExceptionType);
			s.StreamElements("MapID", MapIds);
		}
		#endregion
	};
}