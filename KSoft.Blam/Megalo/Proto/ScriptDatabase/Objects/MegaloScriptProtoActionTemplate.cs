using System;
using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("Name = {Name}, Parent = {Parent}")]
	public sealed class MegaloScriptProtoActionTemplate
		: IO.ITagElementStringNameStreamable
		, IMegaloScriptProtoAction
	{
		public string Name;
		public MegaloScriptProtoActionTemplate Parent;

		public MegaloScriptProtoActionParameters Parameters { get; private set; }

		public MegaloScriptProtoActionTemplate()
		{
			Parameters = new MegaloScriptProtoActionParameters(this);
		}

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var db = (MegaloScriptDatabase)s.Owner;

			s.StreamAttribute("name", ref Name);
			if (db.SerializeActionTemplateReference(s, "template", ref Parent) && Parent == null)
				throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"Action Template '{0}' references undefined template", Name));

			Parameters.Serialize(s);
		}
		#endregion

		#region IMegaloScriptProtoAction Members
		int IMegaloScriptProtoObject.DBID										{ get { return TypeExtensions.kNone; } }
		string IMegaloScriptProtoObject.Name									{ get { return Name; } }
		IReadOnlyList<MegaloScriptProtoParam> IMegaloScriptProtoObjectWithParams.ParameterList		{ get { return Parameters; } }
		IReadOnlyList<MegaloScriptProtoParam> IMegaloScriptProtoObjectWithParams.ParametersBySigId	{ get { throw new NotImplementedException(); } }
		bool IMegaloScriptProtoObjectWithParams.ContainsObjectTypeParameter
		{
			get { return Parameters.ContainsObjectTypeParameter; }
			set { Parameters.ContainsObjectTypeParameter = value; }
		}
		MegaloScriptProtoParam IMegaloScriptProtoObjectWithParams.GetParameterBySigId(int sigId)
		{ throw new NotImplementedException(); }

		IMegaloScriptProtoAction IMegaloScriptProtoAction.Template				{ get { return Parent; } }
		#endregion
	};
}
