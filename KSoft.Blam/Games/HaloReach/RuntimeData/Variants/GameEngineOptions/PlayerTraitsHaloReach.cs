
namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	partial class GameEngineBaseVariantHaloReach
	{
		internal override Blam.RuntimeData.Variants.PlayerTraitsBase NewPlayerTraits()
		{
			return new PlayerTraits();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsDamage
		: Blam.RuntimeData.Variants.PlayerTraitsDamageBase
	{
		public byte DamageResistance,
			BodyMultiplier,
			BodyRechargeRate,
			ShieldMultiplier,
			ShieldRechargeRate0, ShieldRechargeRate1; // ShieldStunDuration, OvershieldRechargeRate?
		public byte HeadshotImmunity, Vampirism, AssassinationImmunity,
			unk9;

		protected override bool ModifiersAreUnchanged { get {
			return DamageResistance == 0 && BodyMultiplier == 0 && BodyRechargeRate == 0 &&
				ShieldMultiplier == 0 && ShieldRechargeRate0 == 0 && ShieldRechargeRate1 == 0;
		} }
		public override bool IsUnchanged { get {
			return ModifiersAreUnchanged && HeadshotImmunity == 0 && Vampirism == 0 && AssassinationImmunity == 0 &&
				unk9 == 0;
		} }

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.Stream(ref DamageResistance, 4);
			s.Stream(ref BodyMultiplier, 3);
			s.Stream(ref BodyRechargeRate, 4);
			s.Stream(ref ShieldMultiplier, 3);
			s.Stream(ref ShieldRechargeRate0, 4);
			s.Stream(ref ShieldRechargeRate1, 4);
			s.Stream(ref HeadshotImmunity, 2);
			s.Stream(ref Vampirism, 3);
			s.Stream(ref AssassinationImmunity, 2);
			s.Stream(ref unk9, 2);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOptUnchangedZero("DamageResistance", ref DamageResistance);
			s.StreamAttributeOptUnchangedZero("BodyMultiplier", ref BodyMultiplier);
			s.StreamAttributeOptUnchangedZero("BodyRechargeRate", ref BodyRechargeRate);
			s.StreamAttributeOptUnchangedZero("ShieldMultiplier", ref ShieldMultiplier);
			s.StreamAttributeOptUnchangedZero("ShieldStunDuration_", ref ShieldRechargeRate0);
			s.StreamAttributeOptUnchangedZero("ShieldRechargeRate_", ref ShieldRechargeRate1);
			s.StreamAttributeOptUnchangedZero("VampirismPercent", ref unk9);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("headshotImmunity", ref HeadshotImmunity);
			s.StreamAttributeOptUnchangedZero("assassinationImmunity", ref AssassinationImmunity);
			s.StreamAttributeOptUnchangedZero("unk9", ref unk9);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsWeapons
		: Blam.RuntimeData.Variants.PlayerTraitsWeaponsBase
	{
		public byte DamageMultiplier, MeleeDamageMultiplier;
		public sbyte InitialPrimaryWeapon, InitialSecondaryWeapon;
		public byte InitialGrenadeCount, InfAmmo,
			RechargingGrenades, WeaponPickupAllowed,
			EquipmentDrop, InfEquipment, EquipmentUsage;
		public sbyte InitialEquipment;

		protected override bool ModifiersAreUnchanged { get {
			return DamageMultiplier == 0 && MeleeDamageMultiplier == 0;
		} }
		bool EquipmentIsUnchanged { get {
			return EquipmentDrop == 0 && InfEquipment == 0 && EquipmentUsage == 0;
		} }
		bool LoadoutIsUnchanged { get {
			return InitialPrimaryWeapon == TypeExtensionsBlam.kUnchanged && InitialSecondaryWeapon == TypeExtensionsBlam.kUnchanged &&
				InitialEquipment == TypeExtensionsBlam.kUnchanged &&
				InitialGrenadeCount == 0;
		} }
		public override bool IsUnchanged { get {
			return ModifiersAreUnchanged && LoadoutIsUnchanged && InfAmmo == 0 && RechargingGrenades == 0 && WeaponPickupAllowed == 0 &&
				EquipmentIsUnchanged;
		} }

		public PlayerTraitsWeapons()
		{
			InitialPrimaryWeapon = InitialSecondaryWeapon = InitialEquipment = TypeExtensionsBlam.kUnchanged;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.Stream(ref DamageMultiplier, 4);
			s.Stream(ref MeleeDamageMultiplier, 4);
			s.Stream(ref InitialPrimaryWeapon);
			s.Stream(ref InitialSecondaryWeapon);
			s.Stream(ref InitialGrenadeCount, 4);
			s.Stream(ref InfAmmo, 2);
			s.Stream(ref RechargingGrenades, 2);
			s.Stream(ref WeaponPickupAllowed, 2);
			s.Stream(ref EquipmentDrop, 2);
			s.Stream(ref InfEquipment, 2);
			s.Stream(ref EquipmentUsage, 2);
			s.Stream(ref InitialEquipment);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOptUnchangedZero("DamageMultiplier", ref DamageMultiplier);
			s.StreamAttributeOptUnchangedZero("MeleeDamageMultiplier", ref MeleeDamageMultiplier);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("pickupAllowed", ref WeaponPickupAllowed);

			s.StreamAttributeOptUnchangedZero("infAmmo", ref InfAmmo);
			#region Equipment
			using (var bm = s.EnterCursorBookmarkOpt("Equipment", this, obj=>!obj.EquipmentIsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("drop", ref EquipmentDrop);
				s.StreamAttributeOptUnchangedZero("infinite", ref InfEquipment);

				s.StreamAttributeOptUnchangedZero("usage", ref EquipmentUsage);

				s.StreamAttributeOptUnchangedZero("rechargingGrenades", ref RechargingGrenades);
			}
			#endregion
			#region Loadout
			using (var bm = s.EnterCursorBookmarkOpt("InitLoadout", this, obj=>!obj.LoadoutIsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchanged("primaryWeapon", ref InitialPrimaryWeapon);
				s.StreamAttributeOptUnchanged("secondaryWeapon", ref InitialSecondaryWeapon);

				s.StreamAttributeOptUnchangedZero("grenadeCount", ref InitialGrenadeCount);

				s.StreamAttributeOptUnchanged("equipment", ref InitialEquipment);
			}
			#endregion
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsMovement
		: Blam.RuntimeData.Variants.PlayerTraitsMovementBase
	{
		public byte Speed, GravityMultiplier, VehicleUsage,
			unknown0; // DoubleJump or AutomaticMomentumUsage?;
		public int JumpMultiplier;

		protected override bool ModifiersAreUnchanged { get {
			return Speed == 0 && GravityMultiplier == 0 && JumpMultiplier.IsNone();
		} }
		bool UsageIsUnchanged { get {
			return VehicleUsage == 0;
		} }
		public override bool IsUnchanged { get {
			return ModifiersAreUnchanged && UsageIsUnchanged && unknown0 == 0;
		} }

		public PlayerTraitsMovement()
		{
			JumpMultiplier = TypeExtensions.kNone;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.Stream(ref Speed, 5);
			s.Stream(ref GravityMultiplier, 4);
			s.Stream(ref VehicleUsage, 4);
			s.Stream(ref unknown0, 2);
			s.StreamIndexPos(ref JumpMultiplier, 9);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOptUnchangedZero("Speed", ref Speed);
			s.StreamAttributeOptUnchangedZero("GravityMultiplier", ref GravityMultiplier);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("unknown0", ref unknown0);

			using (var bm = s.EnterCursorBookmarkOpt("Usage", this, obj=>!obj.UsageIsUnchanged)) if(bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("vehicles", ref VehicleUsage);
			}
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsAppearance
		: Blam.RuntimeData.Variants.PlayerTraitsAppearanceBase
	{
		public byte ActiveCamo,
			Waypoint, GamertagVisible,
			Aura,
			ForcedChangeColor;

		public override bool IsUnchanged { get {
			return ActiveCamo == 0 && Waypoint == 0 && GamertagVisible == 0 &&
				Aura == 0 && ForcedChangeColor == 0;
		} }

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.Stream(ref ActiveCamo, 3);
			s.Stream(ref Waypoint, 2);
			s.Stream(ref GamertagVisible, 2);
			s.Stream(ref Aura, 3);
			s.Stream(ref ForcedChangeColor, 4);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("camo", ref ActiveCamo);
			s.StreamAttributeOptUnchangedZero("waypoint", ref Waypoint);
			s.StreamAttributeOptUnchangedZero("gamertag", ref GamertagVisible);
			s.StreamAttributeOptUnchangedZero("aura", ref Aura);
			s.StreamAttributeOptUnchangedZero("forcedChangeColor", ref ForcedChangeColor);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsSensors
		: Blam.RuntimeData.Variants.PlayerTraitsSensorsBase
	{
		public byte MotionTracker, MotionTrackerRange,
			DirectionalDamageIndicator;

		protected override bool ModifiersAreUnchanged { get {
			return MotionTrackerRange == 0;
		} }
		bool MotionTrackerIsUnchanged { get {
			return MotionTracker == 0 && MotionTrackerRange == 0;
		} }
		public override bool IsUnchanged { get {
			return MotionTrackerIsUnchanged && ModifiersAreUnchanged &&
				DirectionalDamageIndicator == 0;
		} }

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.Stream(ref MotionTracker, 3);
			s.Stream(ref MotionTrackerRange, 3);
			s.Stream(ref DirectionalDamageIndicator, 2);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOptUnchangedZero("MotionTrackerRange", ref MotionTrackerRange);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("directionalDamageIndicator", ref DirectionalDamageIndicator);

			using (var bm = s.EnterCursorBookmarkOpt("MotionTracker", this, obj=>!obj.MotionTrackerIsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("usage", ref MotionTracker);
			}
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraits
		: Blam.RuntimeData.Variants.PlayerTraitsBase
	{
		public PlayerTraits()
		{
			Damage = new PlayerTraitsDamage();
			Weapons = new PlayerTraitsWeapons();
			Movement = new PlayerTraitsMovement();
			Appearance = new PlayerTraitsAppearance();
			Sensors = new PlayerTraitsSensors();
		}
	};
}