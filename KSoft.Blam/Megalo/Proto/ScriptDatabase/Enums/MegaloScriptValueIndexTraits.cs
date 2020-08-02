using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Blam.Megalo.Proto
{
	/// <summary>Type traits for <see cref="MegaloScriptValueBaseType.Index"/></summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum MegaloScriptValueIndexTraits : byte // TypeTraits
	{
		/// <summary>A NONE-able pointer which encodes bitbool to represent 'index != -1' cases</summary>
		PointerHasValue,
		/// <summary>A NONE-able pointer which adds +/-1 when encoding/decoding to avoid signed issues</summary>
		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Pointer,
		/// <summary>A NONE-able pointer which uses the full length of a word so signed issues aren't a problem</summary>
		PointerRaw,
		/// <summary>An absolute, non-NONE-able, reference</summary>
		Reference,

		/// <remarks>2 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}
