using System;
using System.Collections.Generic;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	partial class GameEngineBaseVariantHalo4
	{
		internal override Blam.RuntimeData.Variants.PlayerTraitsBase NewPlayerTraits()
		{
			return new PlayerTraits();
		}
	};
	partial class GameEngineMegaloVariantHalo4
	{
		protected override Blam.RuntimeData.Variants.MegaloVariantPlayerTraitsBase NewMegaloPlayerTraits()
		{
			return new MegaloVariantPlayerTraits(this);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsDamage
		: Blam.RuntimeData.Variants.PlayerTraitsDamageBase
	{
		static readonly Blam.RuntimeData.Variants.OptionalRealArrayInfo kModifiersInfo = new Blam.RuntimeData.Variants.OptionalRealArrayInfo((int)PlayerTraitModifiers.DamageModifiers.kNumberOf)
		{ kValuesEnum = typeof(PlayerTraitModifiers.DamageModifiers) };

		public Blam.RuntimeData.Variants.OptionalRealArray Modifiers { get; private set; }
		public byte HeadshotImmunity, AssassinationImmunity,
			Deathless,
			FastTrackArmor,
			PowerupCancellation;

		protected override bool ModifiersAreUnchanged { get { return Modifiers.AreUnchanged; } }

		public override bool IsUnchanged { get {
			return HeadshotImmunity == 0 && AssassinationImmunity == 0 && Deathless == 0 &&
				FastTrackArmor == 0 && PowerupCancellation == 0 &&
				Modifiers.AreUnchanged;
		} }

		public PlayerTraitsDamage()
		{
			Modifiers = new Blam.RuntimeData.Variants.OptionalRealArray(kModifiersInfo);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			Modifiers.Serialize(s);
			s.Stream(ref HeadshotImmunity, 2);
			s.Stream(ref AssassinationImmunity, 2);
			s.Stream(ref Deathless, 2);
			s.Stream(ref FastTrackArmor, 2);
			s.Stream(ref PowerupCancellation, 2);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{ Modifiers.Serialize(s); }
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("headshotImmunity", ref HeadshotImmunity);
			s.StreamAttributeOptUnchangedZero("assassinationImmunity", ref AssassinationImmunity);
			s.StreamAttributeOptUnchangedZero("deathless", ref Deathless);
			s.StreamAttributeOptUnchangedZero("fastTrackArmor", ref FastTrackArmor);
			s.StreamAttributeOptUnchangedZero("powerupCancellation", ref PowerupCancellation);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsWeapons
		: Blam.RuntimeData.Variants.PlayerTraitsWeaponsBase
	{
		static readonly Blam.RuntimeData.Variants.OptionalRealArrayInfo kModifiersInfo = new Blam.RuntimeData.Variants.OptionalRealArrayInfo((int)PlayerTraitModifiers.WeaponModifiers.kNumberOf)
		{ kValuesEnum = typeof(PlayerTraitModifiers.WeaponModifiers) };

		public Blam.RuntimeData.Variants.OptionalRealArray Modifiers { get; private set; }
		public byte WeaponPickupAllowed,
			InitialGrenadeCount,
			InfAmmo, // 3=BottomlessClip
			EquipmentUsage, EquipmentUsageExceptingAutoTurret, EquipmentDrop, InfEquipment,
			OrdnanceMarkersVisable, OrdnanceRerollAvailable,
			Resourceful, WellEquipped,
			OrdnanceDisabled;
		public sbyte InitialPrimaryWeapon, InitialSecondaryWeapon, InitialEquipment, InitialTacticalPackage, InitialSupportUpgrade;
		public byte Ammopack, Grenadier, ExplodeOnDeathArmorMod;

		protected override bool ModifiersAreUnchanged { get { return Modifiers.AreUnchanged; } }

		bool EquipmentIsUnchanged { get {
			return EquipmentUsage == 0 && EquipmentUsageExceptingAutoTurret == 0 && EquipmentDrop == 0 && InfEquipment == 0;
		} }
		bool OrdnanceIsUnchanged { get {
			return OrdnanceMarkersVisable == 0 && OrdnanceRerollAvailable == 0 && OrdnanceDisabled == 0;
		} }
		bool AppsAreUnchanged { get {
			return Resourceful == 0 && WellEquipped == 0 && Ammopack == 0 && Grenadier == 0 && ExplodeOnDeathArmorMod == 0;
		} }
		bool LoadoutIsUnchanged { get {
			return InitialPrimaryWeapon == TypeExtensionsBlam.kUnchanged && InitialSecondaryWeapon == TypeExtensionsBlam.kUnchanged &&
				InitialEquipment == TypeExtensionsBlam.kUnchanged && InitialTacticalPackage == TypeExtensionsBlam.kUnchanged && InitialSupportUpgrade == TypeExtensionsBlam.kUnchanged &&
				InitialGrenadeCount == 0;
		} }
		public override bool IsUnchanged { get {
			return WeaponPickupAllowed == 0 && InfAmmo == 0 &&
				EquipmentIsUnchanged && OrdnanceIsUnchanged && AppsAreUnchanged && LoadoutIsUnchanged &&
				Modifiers.AreUnchanged;
		} }

		public PlayerTraitsWeapons()
		{
			InitialPrimaryWeapon = InitialSecondaryWeapon = InitialEquipment = InitialTacticalPackage = InitialSupportUpgrade = TypeExtensionsBlam.kUnchanged;
			Modifiers = new Blam.RuntimeData.Variants.OptionalRealArray(kModifiersInfo);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			Modifiers.Serialize(s);
			s.Stream(ref WeaponPickupAllowed, 2);
			s.Stream(ref InitialGrenadeCount, 5);
			s.Stream(ref InfAmmo, 2);
			s.Stream(ref EquipmentUsage, 2);
			s.Stream(ref EquipmentUsageExceptingAutoTurret, 2);
			s.Stream(ref EquipmentDrop, 2);
			s.Stream(ref InfEquipment, 2);

			s.Stream(ref WellEquipped, 2);
			s.Stream(ref Grenadier, 2);
			s.Stream(ref ExplodeOnDeathArmorMod, 2);
			s.Stream(ref OrdnanceMarkersVisable, 2);
			s.Stream(ref Resourceful, 2);
			s.Stream(ref Ammopack, 2);
			s.Stream(ref OrdnanceRerollAvailable, 2); // #TODO_BLAM: verify
			s.Stream(ref OrdnanceDisabled, 2);

			s.Stream(ref InitialPrimaryWeapon);
			s.Stream(ref InitialSecondaryWeapon);
			s.Stream(ref InitialEquipment);
			s.Stream(ref InitialTacticalPackage);
			s.Stream(ref InitialSupportUpgrade);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{ Modifiers.Serialize(s); }
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			bool reading = s.IsReading;

			s.StreamAttributeOptUnchangedZero("pickupAllowed", ref WeaponPickupAllowed);

			s.StreamAttributeOptUnchangedZero("infAmmo", ref InfAmmo);
			#region Equipment
			using (var bm = s.EnterCursorBookmarkOpt("Equipment", this, obj=>!obj.EquipmentIsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("drop", ref EquipmentDrop);
				s.StreamAttributeOptUnchangedZero("infinite", ref InfEquipment);

				s.StreamAttributeOptUnchangedZero("usage", ref EquipmentUsage);
				s.StreamAttributeOptUnchangedZero("usageSansAutoTurret", ref EquipmentUsageExceptingAutoTurret);
			}
			#endregion
			#region Ordnance
			using (var bm = s.EnterCursorBookmarkOpt("Ordnance", this, obj=>!obj.OrdnanceIsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("disabled", ref OrdnanceDisabled);

				s.StreamAttributeOptUnchangedZero("markersVisability", ref OrdnanceMarkersVisable);
				s.StreamAttributeOptUnchangedZero("allowReroll", ref OrdnanceRerollAvailable);
			}
			#endregion
			#region Loadout
			using (var bm = s.EnterCursorBookmarkOpt("InitLoadout", this, obj=>!obj.LoadoutIsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchanged("primaryWeapon", ref InitialPrimaryWeapon);
				s.StreamAttributeOptUnchanged("secondaryWeapon", ref InitialSecondaryWeapon);

				s.StreamAttributeOptUnchangedZero("grenadeCount", ref InitialGrenadeCount);

				s.StreamAttributeOptUnchanged("equipment", ref InitialEquipment);
				s.StreamAttributeOptUnchanged("tacticalPackage", ref InitialTacticalPackage);
				s.StreamAttributeOptUnchanged("supportUpgrade", ref InitialSupportUpgrade);
			}
			#endregion
			#region Apps
			using (var bm = s.EnterCursorBookmarkOpt("Apps", this, obj=>!obj.AppsAreUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("resourceful", ref Resourceful);
				s.StreamAttributeOptUnchangedZero("wellEquipped", ref WellEquipped);

				s.StreamAttributeOptUnchangedZero("ammopack", ref Ammopack);
				s.StreamAttributeOptUnchangedZero("grenadier", ref Grenadier);
				s.StreamAttributeOptUnchangedZero("explodeOnDeath", ref ExplodeOnDeathArmorMod);
			}
			#endregion
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsMovement
		: Blam.RuntimeData.Variants.PlayerTraitsMovementBase
	{
		static readonly Blam.RuntimeData.Variants.OptionalRealArrayInfo kModifiersInfo = new Blam.RuntimeData.Variants.OptionalRealArrayInfo((int)PlayerTraitModifiers.MovementModifiers.kNumberOf)
		{ kValuesEnum = typeof(PlayerTraitModifiers.MovementModifiers) };

		public Blam.RuntimeData.Variants.OptionalRealArray Modifiers { get; private set; }
		public byte VehicleUsage,
			DoubleJump, SprintUsage, AutomaticMomentumUsage,
			VaultingEnabled, Stealthy;

		protected override bool ModifiersAreUnchanged { get { return Modifiers.AreUnchanged; } }

		bool UsageIsUnchanged { get {
			return VehicleUsage == 0 && SprintUsage == 0 && AutomaticMomentumUsage == 0;
		} }
		public override bool IsUnchanged { get {
			return UsageIsUnchanged &&
				DoubleJump == 0 && VaultingEnabled == 0 && Stealthy == 0 &&
				Modifiers.AreUnchanged;
		} }

		public PlayerTraitsMovement()
		{
			Modifiers = new Blam.RuntimeData.Variants.OptionalRealArray(kModifiersInfo);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			Modifiers.Serialize(s);
			s.Stream(ref VehicleUsage, 4);
			s.Stream(ref DoubleJump, 2);
			s.Stream(ref SprintUsage, 2);
			s.Stream(ref AutomaticMomentumUsage, 2);
			s.Stream(ref VaultingEnabled, 2);
			s.Stream(ref Stealthy, 2);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{ Modifiers.Serialize(s); }
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("doubleJump", ref DoubleJump);
			s.StreamAttributeOptUnchangedZero("vaulting", ref VaultingEnabled);
			s.StreamAttributeOptUnchangedZero("stealthy", ref Stealthy);

			using (var bm = s.EnterCursorBookmarkOpt("Usage", this, obj=>!obj.UsageIsUnchanged)) if(bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("vehicles", ref VehicleUsage);

				s.StreamAttributeOptUnchangedZero("sprint", ref SprintUsage);
				s.StreamAttributeOptUnchangedZero("autoMomentum", ref AutomaticMomentumUsage);
			}
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsAppearanceColor
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public bool Transparent;
		public byte Red, Green, Blue;

		public bool IsDefault { get {
			return Transparent == true &&
				Red == byte.MaxValue && Green == byte.MaxValue && Blue == byte.MaxValue;
		} }

		public PlayerTraitsAppearanceColor()
		{
			RevertToDefault();
		}

		public void Set(bool a, byte r, byte g, byte b)
		{
			Transparent = a; Red = r; Green = g; Blue = b;
		}

		public void RevertToDefault()
		{
			Set(true, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Transparent);
			s.Stream(ref Red);
			s.Stream(ref Green);
			s.Stream(ref Blue);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("R", ref Red);
			s.StreamAttribute("G", ref Green);
			s.StreamAttribute("B", ref Blue);
			s.StreamAttributeOpt("A", ref Transparent, Predicates.IsTrue);
		}
		#endregion
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsAppearanceModelVariant
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public bool UseDefault; // when false, uses NameIndex
		public sbyte NameIndex; // msit

		public bool IsDefault { get {
			// #NOTE_BLAM: if use default is true, we really shouldn't need to check name index..
			return UseDefault == true && NameIndex == TypeExtensions.kNone;
		} }

		public PlayerTraitsAppearanceModelVariant()
		{
			RevertToDefault();
		}

		public void RevertToDefault()
		{
			UseDefault = true;
			NameIndex = TypeExtensions.kNone;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref UseDefault);
			s.Stream(ref NameIndex);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt("useDefault", ref UseDefault, Predicates.IsFalse))
				NameIndex = -1;
			else
				s.StreamAttributeOpt("nameIndex", ref NameIndex);
		}
		#endregion
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsAppearance
		: Blam.RuntimeData.Variants.PlayerTraitsAppearanceBase
	{
		static readonly Blam.RuntimeData.Variants.OptionalRealArrayInfo kModifiersInfo = new Blam.RuntimeData.Variants.OptionalRealArrayInfo((int)PlayerTraitModifiers.AppearanceModifiers.kNumberOf)
		{ kValuesEnum = typeof(PlayerTraitModifiers.AppearanceModifiers) };

		public Blam.RuntimeData.Variants.OptionalRealArray Modifiers { get; private set; }
		public byte ActiveCamo,
			Waypoint, GamertagVisible,
			Aura;
		public PlayerTraitsAppearanceColor PrimaryColor = new PlayerTraitsAppearanceColor(),
			SecondaryColor = new PlayerTraitsAppearanceColor();
		public PlayerTraitsAppearanceModelVariant ModelVariant = new PlayerTraitsAppearanceModelVariant();
		public int DeathEffect, LoopingEffect; // mgee; these are sbyte's at runtime
		public byte ShieldHud;

		protected override bool ModifiersAreUnchanged { get { return Modifiers.AreUnchanged; } }

		bool EffectsAreNotDefault { get {
			return DeathEffect != TypeExtensionsBlam.kDefaultOption || LoopingEffect != TypeExtensionsBlam.kDefaultOption;
		} }
		public override bool IsUnchanged { get {
			return ActiveCamo == 0 && Waypoint == 0 && GamertagVisible == 0 &&
				Aura == 0 && ShieldHud == 0 &&
				DeathEffect == TypeExtensionsBlam.kDefaultOption && LoopingEffect == TypeExtensionsBlam.kDefaultOption &&
				PrimaryColor.IsDefault && SecondaryColor.IsDefault && ModelVariant.IsDefault &&
				Modifiers.AreUnchanged;
		} }

		public PlayerTraitsAppearance()
		{
			DeathEffect = LoopingEffect = TypeExtensionsBlam.kDefaultOption;
			Modifiers = new Blam.RuntimeData.Variants.OptionalRealArray(kModifiersInfo);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			Modifiers.Serialize(s);
			s.Stream(ref ActiveCamo, 3);
			s.Stream(ref Waypoint, 2);
			s.Stream(ref GamertagVisible, 2);
			s.Stream(ref Aura, 3);
			s.StreamObject(PrimaryColor);
			s.StreamObject(SecondaryColor);
			s.StreamObject(ModelVariant);
			s.Stream(ref DeathEffect);
			s.Stream(ref LoopingEffect);
			s.Stream(ref ShieldHud, 2);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{ Modifiers.Serialize(s); }
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("camo", ref ActiveCamo);
			s.StreamAttributeOptUnchangedZero("waypoint", ref Waypoint);
			s.StreamAttributeOptUnchangedZero("gamertag", ref GamertagVisible);
			s.StreamAttributeOptUnchangedZero("aura", ref Aura);

			s.StreamAttributeOptUnchangedZero("shieldHud", ref ShieldHud);

			using (var bm = s.EnterCursorBookmarkOpt("Effects", this, obj=>obj.EffectsAreNotDefault)) if(bm.IsNotNull)
			{
				s.StreamAttributeOptDefaultOption("death", ref DeathEffect);
				s.StreamAttributeOptDefaultOption("looping", ref LoopingEffect);
			}
			using (var bm = s.EnterCursorBookmarkOpt("PrimaryColor", PrimaryColor, obj=>!obj.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(PrimaryColor);
			using (var bm = s.EnterCursorBookmarkOpt("SecondaryColor", SecondaryColor, obj=>!obj.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(SecondaryColor);
			using (var bm = s.EnterCursorBookmarkOpt("Model", ModelVariant, obj=>!obj.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(ModelVariant);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class PlayerTraitsSensors
		: Blam.RuntimeData.Variants.PlayerTraitsSensorsBase
	{
		static readonly Blam.RuntimeData.Variants.OptionalRealArrayInfo kModifiersInfo = new Blam.RuntimeData.Variants.OptionalRealArrayInfo((int)PlayerTraitModifiers.SensorsModifiers.kNumberOf)
		{ kValuesEnum = typeof(PlayerTraitModifiers.SensorsModifiers) };

		public Blam.RuntimeData.Variants.OptionalRealArray Modifiers { get; private set; }
		public byte MotionTracker, MotionTrackerWhileZoomed,
			DirectionalDamageIndicator,
			VisionMode, BattleAwareness, ThreatView, AuralEnhancement,
			Nemesis;

		protected override bool ModifiersAreUnchanged { get { return Modifiers.AreUnchanged; } }

		bool MotionTrackerIsUnchanged { get {
			return MotionTracker == 0 && MotionTrackerWhileZoomed == 0;
		} }
		public override bool IsUnchanged { get {
			return MotionTrackerIsUnchanged &&
				DirectionalDamageIndicator == 0 &&
				VisionMode == 0 && BattleAwareness == 0 && ThreatView == 0 && AuralEnhancement == 0 && Nemesis == 0 &&
				Modifiers.AreUnchanged;
		} }

		public PlayerTraitsSensors()
		{
			Modifiers = new Blam.RuntimeData.Variants.OptionalRealArray(kModifiersInfo);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			Modifiers.Serialize(s);
			s.Stream(ref MotionTracker, 3); // "Sensor" option in trait editor
			s.Stream(ref MotionTrackerWhileZoomed, 2);
			s.Stream(ref DirectionalDamageIndicator, 2);

			s.Stream(ref VisionMode, 2);
			s.Stream(ref BattleAwareness, 2); // see other player's health
			s.Stream(ref ThreatView, 2);
			s.Stream(ref AuralEnhancement, 2);
			s.Stream(ref Nemesis, 2);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{ Modifiers.Serialize(s); }
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOptUnchangedZero("directionalDamageIndicator", ref DirectionalDamageIndicator);
			s.StreamAttributeOptUnchangedZero("visionMode", ref VisionMode);
			s.StreamAttributeOptUnchangedZero("battleAwareness", ref BattleAwareness);
			s.StreamAttributeOptUnchangedZero("threatView", ref ThreatView);
			s.StreamAttributeOptUnchangedZero("aura", ref AuralEnhancement);

			s.StreamAttributeOptUnchangedZero("nemesis", ref Nemesis);

			using (var bm = s.EnterCursorBookmarkOpt("MotionTracker", this, obj=>!obj.MotionTrackerIsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptUnchangedZero("usage", ref MotionTracker);
				s.StreamAttributeOptUnchangedZero("usageWhileZoomed", ref MotionTrackerWhileZoomed);
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
			Weapons = new PlayerTraitsWeapons();		// 0x40
			Movement = new PlayerTraitsMovement();		// 0xB4
			Appearance = new PlayerTraitsAppearance();	// 0xD0
			Sensors = new PlayerTraitsSensors();		// 0xEC
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloVariantPlayerTraits
		: Blam.RuntimeData.Variants.MegaloVariantPlayerTraitsBase
	{
		public bool IsHidden,
			IsRuntime; // setting this to true hides the loadout trait overrides

		public MegaloVariantPlayerTraits(Blam.RuntimeData.Variants.GameEngineMegaloVariant variant) : base(variant)
		{
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			base.Serialize(s);

			s.Stream(ref IsHidden);
			s.Stream(ref IsRuntime);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOpt("hidden", ref IsHidden, Predicates.IsTrue);
			s.StreamAttributeOpt("runtime", ref IsRuntime, Predicates.IsTrue);
		}
		#endregion
	};
}