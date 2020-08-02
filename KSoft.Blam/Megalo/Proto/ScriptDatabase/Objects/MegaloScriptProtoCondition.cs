using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloScriptProtoCondition
		: MegaloScriptProtoObjectWithParams
	{
		public List<MegaloScriptProtoParam> Parameters { get; private set; }

		public MegaloScriptProtoCondition()
		{
			Parameters = new List<MegaloScriptProtoParam>();
		}

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			if (!s.StreamAttributeOpt("name", ref Name, Predicates.IsNotNullOrEmpty) &&
				 s.IsReading)
				Name = string.Format(Util.InvariantCultureInfo,
					"Condition{0}", DBID.ToString(Util.InvariantCultureInfo));

			s.StreamableElements("Param", Parameters);
		}
		#endregion

		#region MegaloScriptProtoObjectWithParams Members
		public override IReadOnlyList<MegaloScriptProtoParam> ParameterList { get { return Parameters; } }
		#endregion

		internal void WriteForTryToPort<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptDatabase db)
			where TDoc : class
			where TCursor : class
		{
			// #TODO_BLAM: change impl
			db = db == MegaloScriptDatabase.HaloReach
				? MegaloScriptDatabase.Halo4
				: MegaloScriptDatabase.HaloReach;

			MegaloScriptProtoCondition other;
			if (db.TryGetCondition(Name, out other) && !other.Name.StartsWith("Cond", System.StringComparison.Ordinal))
			{
				s.WriteAttribute("DBID", other.DBID);
				s.WriteAttribute("origDBID", DBID);
			}
		}
	};
}
