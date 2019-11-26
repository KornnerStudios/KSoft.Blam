
namespace KSoft.Blam.Blob
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public enum WellKnownBlob : int
	{
		NotWellKnown,

		ContentHeader,		// chdr
		GameVariant,		// mpvr, s_blffile_game_variant
		MapVariant,			// mapv, s_blffile_map_variant
		LevelInfo,			// levl
		CampaignInfo,		// cmpn
		MapImage,			// mapi

		// char[16]
		// int build number
		// int
		// char[28] build string
		// char[10]
		AuthorHeader,		// athr
		FilmHeader,			// flmh
		FilmData,			// flmd
		// byte[0x28]
		// uint, should be 0x14 (individual key size)
		ServerSignature,	// ssig

		kNumberOf,
	};
}