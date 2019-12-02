
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptTriggerExecutionMode : byte
	{
		Normal, // 'normal'. If activated from a iterated trigger, this can still access the iterator value
		OnEachPlayer, // trigger is executed on every player
		OnEachPlayerRandomly, // player related; gathers a list of all active players, performs some sort of random process on each, settles on one, then runs this trigger on that player X (number of active players?) times
		OnEachTeam, // trigger is executed on every team
		OnEachObject, // trigger is executed on every (multiplayer) object
		OnObjectFilter, // object filter
		OnCandySpawnerFilter, // Added in H4, game object filters; executed on some sort of list in game engine globals
	};
}