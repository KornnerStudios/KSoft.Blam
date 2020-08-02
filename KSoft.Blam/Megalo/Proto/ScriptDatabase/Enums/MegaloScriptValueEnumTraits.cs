using System;

namespace KSoft.Blam.Megalo.Proto
{
	/// <summary>Type traits for <see cref="MegaloScriptValueBaseType.Enum"/></summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum MegaloScriptValueEnumTraits : byte // TypeTraits
	{
		None,
		/// <summary>+/-1 is added to the raw when when encoded/decoded to avoid signed issues</summary>
		HasNoneMember,

		/// <remarks>2 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}
