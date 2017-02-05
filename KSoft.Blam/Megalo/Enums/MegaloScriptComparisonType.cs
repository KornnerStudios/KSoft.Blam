
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptComparisonType : byte
	{
		LessThan,
		GreaterThan,
		Equal,
		LessThanEqual,
		GreaterThanEqual,
		NotEqual,

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}