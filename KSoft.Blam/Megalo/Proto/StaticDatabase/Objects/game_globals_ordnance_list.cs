using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameGlobalsOrdnance
		: MegaloStaticDataNamedMappingObject
	{
		/// <summary>String stored in variant data</summary>
		public string LookupName;

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute("lookupName", ref LookupName);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameGlobalsOrdnanceSetEntry
		: MegaloStaticDataNamedObject
	{
		/// <summary>String stored in variant data</summary>
		public string LookupName;

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute("lookupName", ref LookupName);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameGlobalsOrdnanceList
		: IO.ITagElementStringNameStreamable
	{
		public Dictionary<string, GameGlobalsOrdnance> Types { get; private set; }
		public List<GameGlobalsOrdnanceSetEntry> Sets { get; private set; }

		public bool IsAvailable { get { return Types.Count > 0; } }

		public Dictionary<string, GameGlobalsOrdnance> LookupType { get; private set; }
		public Dictionary<string, GameGlobalsOrdnanceSetEntry> LookupSet { get; private set; }

		internal GameGlobalsOrdnanceList(EngineLimits limits)
		{
			Types = new Dictionary<string, GameGlobalsOrdnance>(limits.GameOrdnanceTypes.MaxCount);
			Sets = new List<GameGlobalsOrdnanceSetEntry>();
		}

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("types")) s.StreamableElements("entry", Types,
					GameGlobalsOrdnance.kAttributeKeyName, GameGlobalsOrdnance.SerializeTypeName);
			using (s.EnterCursorBookmark("sets"))
				s.StreamableElements("entry", Sets);

			if (s.IsReading)
			{
				LookupType = new Dictionary<string, GameGlobalsOrdnance>(Types.Count);
				LookupSet = new Dictionary<string, GameGlobalsOrdnanceSetEntry>(Sets.Count);

				foreach (var kv in Types)
					LookupType.Add(kv.Value.LookupName, kv.Value);
				Sets.ForEach(set => LookupSet.Add(set.LookupName, set));
			}
		}
		#endregion
	};
}