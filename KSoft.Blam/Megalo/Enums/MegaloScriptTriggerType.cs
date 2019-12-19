
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptTriggerType : byte
	{
		Normal,
		InnerLoop, // preserves iterator values
		Initialization,
		LocalInitialization,
		HostMigration, // HostMigration, DoubleHostMigration
		ObjectDeath,
		Local,
		Pregame,
		Incident, // Halo4
	};
}