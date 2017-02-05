
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptNavpointIconType
	{
		None = -1,

		Mic,
		DeathMarker,
		LightningBolt,
		BullsEye,
		Diamond,
		Bomb,
		Flag,
		Skull,
		Crown,
		Vip,
		TerritoryLock,
		Territory, //Alpha,
			Bravo,
			Charlie,
			Delta,
			Echo,
			Foxtrot,
			Gult,
			Hotel,
			India,
		Supply,
		HealthSupply,
		AirDropSupply,
		AmmoSupply,
		Arrow,
		DefendShield,
		Unknown26,
		Unknown27,

		/// <remarks>5 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}