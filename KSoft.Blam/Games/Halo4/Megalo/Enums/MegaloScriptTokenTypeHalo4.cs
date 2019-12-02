
namespace KSoft.Blam.Games.Halo4.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public enum MegaloScriptTokenTypeHalo4 : sbyte
	{
		None = TypeExtensions.kNone, // the engine will never actually stream a None

		AbsolutePlayerIndex, // player's name
		TeamDesignator, // 'team_none' or team designator string
		Object, // 'none' or 'unknown'
		Numeric, // %i
		SignedNumeric, // +%i when positive, else %i
		TimerSeconds, // %i:%02i:%02i or %i:%02i or 0:%02i
	};
}