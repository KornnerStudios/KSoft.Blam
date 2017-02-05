
namespace KSoft.Blam.RuntimeData
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum ContentType : sbyte
	{
		None = -1,
		DLC,
		Save,		// mmiof.bmf
		Screenshot,	// .shot
		Film,		// .film
		FilmClip,	// .clip
		MapVariant,	// .map
		GameVariant,// .game
		Unknown7,	// plst
		Unknown8,	// Halo4. data.cache/Unlocks
	};
}