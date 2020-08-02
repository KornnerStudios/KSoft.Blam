using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	// Technically can be template'd from other Actions, not just ActionTemplates
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("DBID = {DBID}, Name = {Name}, Template = {Template}")]
	public sealed class MegaloScriptProtoAction
		: MegaloScriptProtoObjectWithParams
		, IMegaloScriptProtoAction
	{
		IReadOnlyList<MegaloScriptProtoParam> mInOrderParams;

		public IMegaloScriptProtoAction Template;
		public MegaloScriptProtoActionParameters Parameters { get; private set; }
		public MegaloScriptProtoActionFlags Flags;
		/// <summary>Is this action really a wrapper for a virtual trigger? Eg, a branch</summary>
		/// <remarks>Auto-calculated</remarks>
		public bool ContainsVirtualTriggerParameter;

		public MegaloScriptProtoAction()
		{
			Parameters = new MegaloScriptProtoActionParameters(this);
		}

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var db = (MegaloScriptDatabase)s.Owner;

			if (!s.StreamAttributeOpt("name", ref Name, Predicates.IsNotNullOrEmpty) &&
				 s.IsReading)
				Name = string.Format(Util.InvariantCultureInfo,
					"Action{0}", DBID.ToString(Util.InvariantCultureInfo));
			if (db.SerializeProtoActionReference(s, "template", ref Template) && Template == null)
				throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"Action '{0}' references undefined proto action {1}", Name, Template));
			s.StreamAttributeEnumOpt("flags", ref Flags, f => f != 0);

			Parameters.Serialize(s);
		}
		#endregion

		/// <summary>Create the ordered params list from the action parameter hierarchy</summary>
		internal void InitializeParameterList()
		{
			if (Template == null)
				mInOrderParams = Parameters;
			else
			{
				var list = new List<MegaloScriptProtoParam>(Parameters.Count);
				Parameters.BuildOrderedParamsList(list);
				mInOrderParams = list;
			}
		}

		#region IMegaloScriptProtoAction Members
		public override IReadOnlyList<MegaloScriptProtoParam> ParameterList	{ get { return mInOrderParams; } }

		IMegaloScriptProtoAction IMegaloScriptProtoAction.Template			{ get { return Template; } }
		#endregion

		internal void WriteForTryToPort<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptDatabase db)
			where TDoc : class
			where TCursor : class
		{
			// #TODO_IMPLEMENT: change impl
			db = db == MegaloScriptDatabase.HaloReach
				? MegaloScriptDatabase.Halo4
				: MegaloScriptDatabase.HaloReach;

			MegaloScriptProtoAction other;
			if (db.TryGetAction(Name, out other) && !other.Name.StartsWith("Action", System.StringComparison.Ordinal))
			{
				s.WriteAttribute("DBID", other.DBID);
				s.WriteAttribute("origDBID", DBID);
			}
		}
	};
}
