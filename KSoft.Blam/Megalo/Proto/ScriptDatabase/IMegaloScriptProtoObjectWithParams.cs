using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public interface IMegaloScriptProtoObjectWithParams
		: IMegaloScriptProtoObject
	{
		/// <summary>Parameters ordered as they're defined in the database/appear in bitstreams</summary>
		IReadOnlyList<MegaloScriptProtoParam> ParameterList { get; }
		/// <summary>Parameters ordered by their <see cref="MegaloScriptProtoParam.SigId"/></summary>
		IReadOnlyList<MegaloScriptProtoParam> ParametersBySigId { get; }

		/// <summary>Does this object contain a parameter that is an <see cref="MegaloScriptValueIndexTarget.ObjectType"/>?</summary>
		/// <remarks>Auto-calculated</remarks>
		bool ContainsObjectTypeParameter { get; set; }

		MegaloScriptProtoParam GetParameterBySigId(int sigId);
	};
}