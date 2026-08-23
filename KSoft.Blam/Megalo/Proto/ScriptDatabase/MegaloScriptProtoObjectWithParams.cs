using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("DBID = {DBID}, Name = {Name}")]
	public abstract class MegaloScriptProtoObjectWithParams
		: IO.ITagElementStringNameStreamable
		, IMegaloScriptProtoObjectWithParams
	{
		public int DBID;
		public string Name = null!;

		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("DBID", ref DBID);
		}
		#endregion

		#region IMegaloScriptProtoObjectWithParams Members
		int IMegaloScriptProtoObject.DBID => DBID;
		string IMegaloScriptProtoObject.Name => Name;
		public abstract IReadOnlyList<MegaloScriptProtoParam> ParameterList { get; }
		public IReadOnlyList<MegaloScriptProtoParam> ParametersBySigId	{ get; private set; } = null!;
		public bool ContainsObjectTypeParameter							{ get; set; }
		/// <summary>Do the SigId's not match the parameter list order?</summary>
		public bool ParametersRequireAccessBySigId => ParametersBySigId != null;
		public MegaloScriptProtoParam GetParameterBySigId(int sigId)
		{
			if (ParametersRequireAccessBySigId)
			{
				return ParametersBySigId[sigId];
			}

			return ParameterList[sigId];
		}

		internal void SetParamsBySigId(IReadOnlyList<MegaloScriptProtoParam> bySigId)
		{ ParametersBySigId = bySigId; }
		#endregion
	};
}
