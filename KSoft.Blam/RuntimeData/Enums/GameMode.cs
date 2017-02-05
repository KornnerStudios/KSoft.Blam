
namespace KSoft.Blam.RuntimeData
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum GameMode : byte // GameType
	{
		None,
		Campaign,
		Survival,
		Matchmaking,
		Forge,
		Theater,
		Unknown6, // Halo4. Firefight?
	};
}