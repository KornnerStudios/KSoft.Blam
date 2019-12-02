using System;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum MegaloScriptTriggerEntryPoints
	{
		Initialization = 1<<0,
		LocalInitialization = 1<<1,
		HostMigration = 1<<2,
		DoubleHostMigration = 1<<3,
		ObjectDeathEvent = 1<<4,
		Local = 1<<5,
		Pregame = 1<<6,
		Incident = 1<<7,
	};
}