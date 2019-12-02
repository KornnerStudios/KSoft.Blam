
namespace KSoft.Blam.Games.Halo4.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	sealed partial class MegaloScriptModelHalo4
		: Blam.Megalo.Model.MegaloScriptModel
	{
		public MegaloScriptModelHalo4(Blam.RuntimeData.Variants.GameEngineVariant variantManager, RuntimeData.Variants.GameEngineMegaloVariantHalo4 variant) : base(variantManager, variant)
		{
		}
	};

	#region Engine Definition struct
	//////////////////////////////////////////////////////////////////////////
	// Engine Definition 0xFCC8
	// 0x0		Triggers
	// 0x804	Conditions
	// 0x2C08	Actions
	// 0x810C	GameStatistics
	// 0x8128	GlobalVariables
	// 0x8208	PlayerVariables
	// 0x8278	ObjectVariables
	// 0x82E8	TeamVariables
	// 0x835C	HudWidgets
	// 0x8364	ObjectFilters
	// 0x8468	GameObjectFilterCount
	// 0x846C	CandySpawnerFilters
	// 0x848C	InitializationTriggerIndex
	// 0x8490	LocalInitializationTriggerIndex
	// 0x8494	HostMigrationTriggerIndex
	// 0x8498	DoubleHostMigrationTriggerIndex
	// 0x849C	ObjectDeathEventTriggerIndex
	// 0x84A0	LocalTriggerIndex
	// 0x84A4	PregameTriggerIndex
	// 0x84A8	IncidentTriggerIndex
	// 0x84AC	ObjectTypeReferences
	#endregion

	#region Condition struct
	//////////////////////////////////////////////////////////////////////////
	// Condition
	// 0xC Type					byte
	// 0xD Inverted				bool
	// 0xE UnionGroup			byte
	// 0xF ExecuteBeforeAction	byte
	#endregion
}