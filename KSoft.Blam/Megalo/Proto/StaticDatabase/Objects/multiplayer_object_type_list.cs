using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MultiplayerObjectType
		: MegaloStaticDataNamedObject
	{
		public string GroupTag = "";
		public string TagName = "";

		public bool IsValid { get {
			return !string.IsNullOrEmpty(GroupTag) && !string.IsNullOrEmpty(TagName);
		} }

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOpt("groupTag", ref GroupTag, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("tagName", ref TagName, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MultiplayerObjectMapping
		: MegaloStaticDataReMappingObject
	{
		public string DescriptionId = "";
		public string HeaderId = "";
		public string HelpId = "";
		public string IconId = "";

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOpt("desc", ref DescriptionId, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("header", ref HeaderId, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("help", ref HelpId, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("icon", ref IconId, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MultiplayerObjectSetEntry
		: MegaloStaticDataNamedObject
	{
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MultiplayerObjectTypeList
		: IO.ITagElementStringNameStreamable
	{
		public List<MultiplayerObjectType> Types { get; private set; }
		public Dictionary<string, MultiplayerObjectMapping> Weapons { get; private set; }
		public Dictionary<string, MultiplayerObjectMapping> Vehicles { get; private set; }
		public Dictionary<string, MultiplayerObjectMapping> Grenades { get; private set; }
		public Dictionary<string, MultiplayerObjectMapping> Equipment { get; private set; }
		public List<MultiplayerObjectSetEntry> WeaponSets { get; private set; }
		public List<MultiplayerObjectSetEntry> VehicleSets { get; private set; }

		internal MultiplayerObjectTypeList(EngineLimits limits)
		{
			Types = new List<MultiplayerObjectType>(limits.MultiplayerObjectTypes.MaxCount);
			Weapons = new Dictionary<string, MultiplayerObjectMapping>();
			Vehicles = new Dictionary<string, MultiplayerObjectMapping>();
			Grenades = new Dictionary<string, MultiplayerObjectMapping>();
			Equipment = new Dictionary<string, MultiplayerObjectMapping>();
			WeaponSets = new List<MultiplayerObjectSetEntry>();
			VehicleSets = new List<MultiplayerObjectSetEntry>();
		}

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("types"))
				s.StreamableElements("entry", Types);
			using (s.EnterCursorBookmark("weapons")) s.StreamableElements("entry", Weapons,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName);
			using (s.EnterCursorBookmark("vehicles")) s.StreamableElements("entry",  Vehicles,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName);
			using (s.EnterCursorBookmark("grenades")) s.StreamableElements("entry", Grenades,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName);
			using (s.EnterCursorBookmark("equipment")) s.StreamableElements("entry", Equipment,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName);
			using (s.EnterCursorBookmark("weaponSets"))
				s.StreamableElements("entry", WeaponSets);
			using (s.EnterCursorBookmark("vehicleSets"))
				s.StreamableElements("entry", VehicleSets);

			if (s.IsReading)
			{
				Types.TrimExcess();
			}
		}
		#endregion
	};
}