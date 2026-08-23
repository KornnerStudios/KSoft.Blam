using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MultiplayerObjectType
		: MegaloStaticDataNamedObject
	{
		public string GroupTag = "";
		public string TagName = "";

		public bool IsValid
			=> !string.IsNullOrEmpty(GroupTag) && !string.IsNullOrEmpty(TagName);

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			string? group_tag = GroupTag;
			s.StreamAttributeOpt("groupTag", ref group_tag, Predicates.IsNotNullOrEmpty);
			GroupTag = group_tag ?? "";
			string? tag_name = TagName;
			s.StreamAttributeOpt("tagName", ref tag_name, Predicates.IsNotNullOrEmpty);
			TagName = tag_name ?? "";
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

			string? description_id = DescriptionId;
			s.StreamAttributeOpt("desc", ref description_id, Predicates.IsNotNullOrEmpty);
			DescriptionId = description_id ?? "";
			string? header_id = HeaderId;
			s.StreamAttributeOpt("header", ref header_id, Predicates.IsNotNullOrEmpty);
			HeaderId = header_id ?? "";
			string? help_id = HelpId;
			s.StreamAttributeOpt("help", ref help_id, Predicates.IsNotNullOrEmpty);
			HelpId = help_id ?? "";
			string? icon_id = IconId;
			s.StreamAttributeOpt("icon", ref icon_id, Predicates.IsNotNullOrEmpty);
			IconId = icon_id ?? "";
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
		public Dictionary<string, MultiplayerObjectMapping> Weapons { get; private set; } = new();
		public Dictionary<string, MultiplayerObjectMapping> Vehicles { get; private set; } = new();
		public Dictionary<string, MultiplayerObjectMapping> Grenades { get; private set; } = new();
		public Dictionary<string, MultiplayerObjectMapping> Equipment { get; private set; } = new();
		public List<MultiplayerObjectSetEntry> WeaponSets { get; private set; } = new();
		public List<MultiplayerObjectSetEntry> VehicleSets { get; private set; } = new();

		internal MultiplayerObjectTypeList(EngineLimits limits)
		{
			Types = new List<MultiplayerObjectType>(limits.MultiplayerObjectTypes.MaxCount);
		}

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("types"))
			{
				s.StreamableElements("entry", Types);
			}

			using (s.EnterCursorBookmark("weapons")) { s.StreamableElements("entry", Weapons,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName); }
			using (s.EnterCursorBookmark("vehicles")) { s.StreamableElements("entry",  Vehicles,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName); }
			using (s.EnterCursorBookmark("grenades")) { s.StreamableElements("entry", Grenades,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName); }
			using (s.EnterCursorBookmark("equipment")) { s.StreamableElements("entry", Equipment,
					MultiplayerObjectMapping.kAttributeKeyName, MultiplayerObjectMapping.SerializeTypeName); }

			using (s.EnterCursorBookmark("weaponSets"))
			{
				s.StreamableElements("entry", WeaponSets);
			}
			using (s.EnterCursorBookmark("vehicleSets"))
			{
				s.StreamableElements("entry", VehicleSets);
			}

			if (s.IsReading)
			{
				Types.TrimExcess();
			}
		}
		#endregion
	};
}
