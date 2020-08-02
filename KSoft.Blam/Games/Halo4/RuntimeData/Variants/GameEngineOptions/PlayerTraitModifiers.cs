using System.Diagnostics.CodeAnalysis;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants.PlayerTraitModifiers
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum DamageModifiers
	{
		DamageResistance,
		ShieldMultiplier,
		BodyMultiplier,
		ShieldStunDuration,
		ShieldRechargeRate,
		BodyRechargeRate,
		OvershieldRechargeRate,
		VampirismPercent,
		ExplosiveDamageResistance,
		WheelmanArmorVehicleStunTimeModifier,
		WheelmanArmorVehicleRechargeTimeModifier,
		WheelmanArmorVehicleEmpDisabledTimeModifier,
		FallDamageMultiplier,

		kNumberOf = 13,
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum WeaponModifiers
	{
		DamageMultiplier,
		MeleeDamageMultiplier,
		GrenadeRechargeSecondsFrag,
		GrenadeRechargeSecondsPlasma,
		GrenadeRechargeSecondsSpike,
		HeroEquipmentEnergyUseRateModifier,
		HeroEquipmentEnergyRechargeDelayModifier,
		HeroEquipmentEnergyRechargeRateModifier,
		HeroEquipmentInitialEnergyModifier,
		EquipmentEnergyUseRateModifier,
		EquipmentEnergyRechargeDelayModifier,
		EquipmentEnergyUseRechargeRateModifier,
		EquipmentInitialEnergyModifier,
		SwitchSpeedModifier,
		ReloadSpeedModifier,
		OrdnancePointsModifier,
		ExplosiveAreaOfEffectRadiusModifier,
		GunnerArmorModifier,
		StabilityArmorModifier,
		DropReconWarningSeconds,
		DropReconDistanceModifier,
		AssassinationSpeedModifier,

		kNumberOf = 22,
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum MovementModifiers
	{
		Speed,
		GravityMultiplier,
		JumpMultiplier,
		TurnSpeedMultiplier,

		kNumberOf = 4,
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum AppearanceModifiers
	{
		PlayerScale,

		kNumberOf = 1,
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum SensorsModifiers
	{
		MotionTrackerRange,
		NemesisDuration,

		kNumberOf = 2,
	};
}
