
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptTriggerType : byte
	{
		Normal, // 'normal'?
		Subroutine, // preserves iterator values
		Initialization,
		LocalInitialization, // Haven't verified (nor seen used yet)
		HostMigration, // HostMigration, DoubleHostMigration
		ObjectDeathEvent,
		Local,
		Pregame,
		Incident, // Halo4
	};
}