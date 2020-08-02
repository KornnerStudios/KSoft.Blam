using System;

namespace KSoft.Blam.Megalo.Proto
{
	/// <summary>Type parameters for <see cref="MegaloScriptValueBaseType.VarReference"/></summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptVarReferenceType : byte // TypeParameter
	{
		Custom,
		Player,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Object,
		Team,
		Timer,
		Any,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}
