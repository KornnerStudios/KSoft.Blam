using System;

namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum MegaloScriptObjectFilterValidParameters
	{
		ObjectType = 1<<0,
		Team = 1<<1,
		Numeric = 1<<2,
	};
}