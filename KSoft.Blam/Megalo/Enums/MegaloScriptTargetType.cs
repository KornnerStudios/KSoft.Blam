
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptTargetType : byte
	{
		Team,
		Player,
		None, // always returns -1
	};
}