
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public interface IMegaloScriptProtoAction
		: IMegaloScriptProtoObjectWithParams
	{
		IMegaloScriptProtoAction Template { get; }
		/// <summary>Parameter hierarchy</summary>
		MegaloScriptProtoActionParameters Parameters { get; }
	};
}