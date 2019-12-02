using System;

namespace KSoft.Blam.Megalo.Proto
{
	/// <summary>Type parameters for <see cref="MegaloScriptValueBaseType.Index"/></summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptValueIndexTarget : byte // TypeParameter
	{
		Undefined, // Requires a bit length, but we don't verify this!
		// scripting
		Trigger,
		// static
		ObjectType,
		Name,
		Sound,
		Incident,
		Icon, // idk...
		Medal, // H4
		Ordnance, // H4. TODO: There may not be any actual instances of an Ordnance index...
		// variant
		LoadoutPalette,
		Option,
		String,
		PlayerTraits,
		Statistic,
		Widget,
		ObjectFilter,
		GameObjectFilter, // H4. Currently not used in any script values, only for Trigger utils (which contains refs)
		// If you remove a member, chances are the required bits will drop to 4

		/// <remarks>5 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}