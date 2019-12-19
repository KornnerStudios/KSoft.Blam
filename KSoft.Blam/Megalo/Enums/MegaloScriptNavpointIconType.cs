
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptNavpointIconType
	{
		None = -1,

		Speaker,
		DeadTeammate,
		LightningBolt,
		Target,
		Destination,
		Bomb,
		Flag,
		Skull,
		Hill,
		Vip,
		Padlock,
		Territory, //Alpha,
			Bravo,
			Charlie,
			Delta,
			Echo,
			Foxtrot,
			Gult,
			Hotel,
			India,
		Custom0,//Supply,
		Custom1,//HealthSupply,
		Custom2,//AirDropSupply,
		Custom3,//AmmoSupply,
		Custom4,//Arrow,
		Custom5,//DefendShield,
		Custom6,
		Custom7,

		/// <remarks>5 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}