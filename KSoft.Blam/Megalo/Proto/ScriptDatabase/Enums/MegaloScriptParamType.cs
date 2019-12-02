
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptParamType
	{
		Input,
		/// <summary>Parameter is set before return</summary>
		Output,
		/// <summary>Parameter is expected to have a valid value on enter, and is set/changed on return</summary>
		InOut,
	};
}