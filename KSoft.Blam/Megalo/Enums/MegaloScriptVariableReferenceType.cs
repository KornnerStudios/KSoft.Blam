
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptVariableReferenceType : sbyte
	{
		Undefined = TypeExtensions.kNone,
		Custom,
		Player,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Object,
		Team,
		Timer,

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}
