
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptVariableSet : byte // TypeTraits
	{
		Globals,
		Player,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Object,
		Team,

		/// <remarks>2 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}
