using System;

namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptModelObjectType
	{
		None,
		Value,
		UnionGroup,
		Condition,
		Action,
		Trigger,
		VirtualTrigger,
		Pragma,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}