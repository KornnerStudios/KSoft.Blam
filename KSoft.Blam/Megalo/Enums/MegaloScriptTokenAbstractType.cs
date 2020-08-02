
namespace KSoft.Blam.Megalo
{
	/// <summary>Game-agnostic token type</summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptTokenAbstractType
	{
		None = -1, // the engine will never actually stream a None

		Player, // player's name
		Team, // 'team_none' or team designator string
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Object, // 'none' or 'unknown'
		Numeric, // %i
		SignedNumeric, // +%i when positive, else %i
		Timer, // %i:%02i:%02i or %i:%02i or 0:%02i
	};
}
