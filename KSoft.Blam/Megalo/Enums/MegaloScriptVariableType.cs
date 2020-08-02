
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptVariableType : byte // TypeParameter
	{
		Numeric,
		Timer,
		Team,
		Player,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Object,

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}
