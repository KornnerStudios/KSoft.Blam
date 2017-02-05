
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptPlayerFilterType
	{
		None, // compare always returns false
		Anyone, // compare always returns true
		Allies, // (this/owner team filter?)
		Enemies, // (enemy team filter?)
		PlayerMask,
		NoOne, // compare always returns false; no_one?
	};
}