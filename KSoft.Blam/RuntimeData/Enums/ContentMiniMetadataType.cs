namespace KSoft.Blam.RuntimeData
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public enum ContentMiniMetadataType : byte
	{
		DLC = (byte)'d',
		Save = (byte)'s',
		Screenshot = (byte)'t',
		Film = (byte)'f',
		FilmClip = (byte)'c',
		MapVariant = (byte)'m',
		GameVariant = (byte)'g',
		Unknown7 = (byte)'p',
		Unknown8 = (byte)'o',
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	static class ContentMiniMetadataTypeUtils
	{
		public const string DLC = "d";			// dlc
		public const string Save = "s";			// save
		public const string Screenshot = "t";	// shot
		public const string Film = "f";			// film
		public const string FilmClip = "c";		// clip
		public const string MapVariant = "m";	// mvar
		public const string GameVariant = "g";	// gvar
		public const string Unknown7 = "p";		// plst
		public const string Unknown8 = "o";		// spartan ops?
	};
}

namespace KSoft.Blam
{
	using ContentMiniMetadataType = RuntimeData.ContentMiniMetadataType;
	using ContentMiniMetadataTypeUtils = RuntimeData.ContentMiniMetadataTypeUtils;

	partial class TypeExtensionsBlam
	{
		internal static string ToEncodingPrefix(this ContentMiniMetadataType type)
		{
			return type switch
			{
				ContentMiniMetadataType.DLC =>			ContentMiniMetadataTypeUtils.DLC,
				ContentMiniMetadataType.Save =>			ContentMiniMetadataTypeUtils.Save,
				ContentMiniMetadataType.Screenshot =>	ContentMiniMetadataTypeUtils.Screenshot,
				ContentMiniMetadataType.Film =>			ContentMiniMetadataTypeUtils.Film,
				ContentMiniMetadataType.FilmClip =>		ContentMiniMetadataTypeUtils.FilmClip,
				ContentMiniMetadataType.MapVariant =>	ContentMiniMetadataTypeUtils.MapVariant,
				ContentMiniMetadataType.GameVariant =>	ContentMiniMetadataTypeUtils.GameVariant,
				ContentMiniMetadataType.Unknown7 =>		ContentMiniMetadataTypeUtils.Unknown7,
				ContentMiniMetadataType.Unknown8 =>		ContentMiniMetadataTypeUtils.Unknown8,

				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}
		internal static bool IsValid(this ContentMiniMetadataType type)
		{
			return type switch
			{
				ContentMiniMetadataType.DLC or
				ContentMiniMetadataType.Save or
				ContentMiniMetadataType.Screenshot or
				ContentMiniMetadataType.Film or
				ContentMiniMetadataType.FilmClip or
				ContentMiniMetadataType.MapVariant or
				ContentMiniMetadataType.GameVariant or
				ContentMiniMetadataType.Unknown7 or
				ContentMiniMetadataType.Unknown8
				=> true,

				_ => false,
			};
		}
		internal static string ToFileExtension(this ContentMiniMetadataType type)
		{
			return type switch
			{
				ContentMiniMetadataType.Save =>			"bmf",
				ContentMiniMetadataType.Screenshot =>	"shot",
				ContentMiniMetadataType.Film =>			"film",
				ContentMiniMetadataType.FilmClip =>		"clip",
				ContentMiniMetadataType.MapVariant =>	"map",
				ContentMiniMetadataType.GameVariant =>	"game",

				_ => "",
			};
		}
		internal static string ToFileNameAndExtension(this ContentMiniMetadataType type)
		{
			return type switch
			{
				ContentMiniMetadataType.Save =>			"mmiof.bmf",
				ContentMiniMetadataType.Screenshot =>	"screen.shot",
				ContentMiniMetadataType.Film =>			"feature.film",
				ContentMiniMetadataType.FilmClip =>		"snippit.clip",
				ContentMiniMetadataType.MapVariant =>	"sandbox.map",
				ContentMiniMetadataType.GameVariant =>	"variant",
				ContentMiniMetadataType.Unknown8 =>		"data.cache",
				_ => "",
			};
		}
	};
}
