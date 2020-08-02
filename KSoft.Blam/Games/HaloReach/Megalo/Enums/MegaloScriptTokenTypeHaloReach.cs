
namespace KSoft.Blam.Games.HaloReach.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public enum MegaloScriptTokenTypeHaloReach : sbyte
	{
		None = TypeExtensions.kNone, // the engine will never actually stream a None

		AbsolutePlayerIndex, // player's name
		TeamDesignator, // 'team_none' or team designator string
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Object, // 'none' or 'unknown'
		Numeric, // %i
		TimerSeconds, // %i:%02i:%02i or %i:%02i or 0:%02i
	};
}
