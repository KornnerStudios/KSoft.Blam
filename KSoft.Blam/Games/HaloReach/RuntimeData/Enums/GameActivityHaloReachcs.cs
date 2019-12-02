
namespace KSoft.Blam.Games.HaloReach.RuntimeData
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum GameActivity : sbyte
	{
		None = TypeExtensions.kNone,
		Activities,
		Campaign,
		Survival,
		Matchmaking,
		Forge,
		Theater,

		kNumberOf
	};
}