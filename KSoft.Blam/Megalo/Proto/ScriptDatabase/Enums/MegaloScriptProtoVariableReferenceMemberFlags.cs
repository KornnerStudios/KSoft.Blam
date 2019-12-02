using System;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[Flags]
	public enum MegaloScriptProtoVariableReferenceMemberFlags
	{
		Readonly = 1<<0,
		HasDataType = 1<<1,
		HasDataValue = 1<<2,
	};
}