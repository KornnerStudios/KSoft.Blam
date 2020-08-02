using System;

namespace KSoft.Blam.RuntimeData
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class ContentAuthor
		: IO.IBitStreamSerializable
		, IO.IEndianStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		const ushort kDeveloperNameCode = 0xA600; // includes null terminator

		const int kNameLength = 16;
		static readonly Memory.Strings.StringStorage kNameStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageType.CString, fixedLength: kNameLength);
		public static readonly Text.StringStorageEncoding kNameEncoding = new Text.StringStorageEncoding(
			kNameStorage);

		public DateTime TimeStamp;
		public ulong Xuid;
		public string Name = "";
		public bool IsOnlineId;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public bool Name_NeedsDevHack { get { return Name.Length == 1 && Name[0] == 0x3F; } }

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref TimeStamp);
			s.Stream(ref Xuid);
			if (s.IsWriting && Name_NeedsDevHack)
				s.Write(kDeveloperNameCode);
			else
				s.Stream(ref Name, Memory.Strings.StringStorage.CStringAscii, maxLength: kNameLength - 1);
			s.Stream(ref IsOnlineId);
		}
		#endregion

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref TimeStamp, isUnixTime: true);
			s.Stream(ref Xuid);
			s.Stream(ref Name, kNameEncoding); // #REVIEW_BLAM: do we need to do Name_NeedsDevHack here?
			s.Stream(ref IsOnlineId);
			s.Pad24();
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("timeStamp", ref TimeStamp);
			s.StreamAttributeOpt("xuid", ref Xuid, Predicates.IsNotZero, NumeralBase.Hex);
			s.StreamAttributeOpt("onlineId", ref IsOnlineId, Predicates.IsTrue);
			s.StreamAttributeOpt("name", ref Name, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};
}
