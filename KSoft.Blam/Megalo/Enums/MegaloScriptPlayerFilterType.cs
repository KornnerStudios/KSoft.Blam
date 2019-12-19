
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptPlayerFilterType
	{
		NoOne, // compare always returns false
		Everyone, // compare always returns true
		AlliesOfTeam,
		EnemiesOfTeam,
		PlayerMask,
		Default, // players: use their traits. objects: same as NoOne
	};
}