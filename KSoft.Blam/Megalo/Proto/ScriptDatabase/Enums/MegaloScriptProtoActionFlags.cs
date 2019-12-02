using System;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum MegaloScriptProtoActionFlags
	{
		Deprecated = 1<<0,
		/// <summary>Is this action only in/for non-retail builds?</summary>
		DebugOnly = 1<<1,
	};
}