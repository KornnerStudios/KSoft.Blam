using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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
		[Contracts.Pure]
		internal static string ToEncodingPrefix(this RuntimeData.ContentMiniMetadataType type)
		{
			switch (type)
			{
				case ContentMiniMetadataType.DLC:			return ContentMiniMetadataTypeUtils.DLC;
				case ContentMiniMetadataType.Save:			return ContentMiniMetadataTypeUtils.Save;
				case ContentMiniMetadataType.Screenshot:	return ContentMiniMetadataTypeUtils.Screenshot;
				case ContentMiniMetadataType.Film:			return ContentMiniMetadataTypeUtils.Film;
				case ContentMiniMetadataType.FilmClip:		return ContentMiniMetadataTypeUtils.FilmClip;
				case ContentMiniMetadataType.MapVariant:	return ContentMiniMetadataTypeUtils.MapVariant;
				case ContentMiniMetadataType.GameVariant:	return ContentMiniMetadataTypeUtils.GameVariant;
				case ContentMiniMetadataType.Unknown7:		return ContentMiniMetadataTypeUtils.Unknown7;
				case ContentMiniMetadataType.Unknown8:		return ContentMiniMetadataTypeUtils.Unknown8;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		[Contracts.Pure]
		internal static bool IsValid(this RuntimeData.ContentMiniMetadataType type)
		{
			switch (type)
			{
				case ContentMiniMetadataType.DLC:
				case ContentMiniMetadataType.Save:
				case ContentMiniMetadataType.Screenshot:
				case ContentMiniMetadataType.Film:
				case ContentMiniMetadataType.FilmClip:
				case ContentMiniMetadataType.MapVariant:
				case ContentMiniMetadataType.GameVariant:
				case ContentMiniMetadataType.Unknown7:
				case ContentMiniMetadataType.Unknown8:
					return true;

				default: return false;
			}
		}
		[Contracts.Pure]
		internal static string ToFileExtension(this RuntimeData.ContentMiniMetadataType type)
		{
			switch (type)
			{
				case ContentMiniMetadataType.Save:			return "bmf";
				case ContentMiniMetadataType.Screenshot:	return "shot";
				case ContentMiniMetadataType.Film:			return "film";
				case ContentMiniMetadataType.FilmClip:		return "clip";
				case ContentMiniMetadataType.MapVariant:	return "map";
				case ContentMiniMetadataType.GameVariant:	return "game";

				default: return "";
			}
		}
		[Contracts.Pure]
		internal static string ToFileNameAndExtension(this RuntimeData.ContentMiniMetadataType type)
		{
			switch (type)
			{
				case ContentMiniMetadataType.Save:			return "mmiof.bmf";
				case ContentMiniMetadataType.Screenshot:	return "screen.shot";
				case ContentMiniMetadataType.Film:			return "feature.film";
				case ContentMiniMetadataType.FilmClip:		return "snippit.clip";
				case ContentMiniMetadataType.MapVariant:	return "sandbox.map";
				case ContentMiniMetadataType.GameVariant:	return "variant";
				case ContentMiniMetadataType.Unknown8:		return "data.cache";

				default: return "";
			}
		}
	};
}