using System;

namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	using MegaloVariantTitleUpdateFlagsBitStreamer = IO.EnumBitStreamer<MegaloVariantTitleUpdateFlags>;
	using LocaleStringTableInfo = Localization.StringTables.LocaleStringTableInfo;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum MegaloVariantTitleUpdateFlags : uint
	{
		DamageBleedThroughEnabled = 1<<0,
		StickyGrenadeShedOnArmorLockDisabled = 1<<1,
		StickyGrenadeOnArmorLockEnabled = 1<<2,
		ActiveCamoModifiersEnabled = 1<<3,
		SwordBlockWithSwordOnlyEnabled = 1<<4,
		AutomaticMagnumEnabled = 1<<5,
		Unused6 = 1<<6,
		Unused7 = 1<<7,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloVariantTU1
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		/*
Make damage bleed to health through shields possible.
Make reticle bloom configurable.

Modify Armor Lock.
Sticky grenade will not be nullified if you use Armor Lock after being stuck.
Damage received while in Armor Lock is transferred to remaining Armor Ability energy.
How much the damage depletes your Armor Lock energy is configurable.

Modify Active Camo.
Reduce the bonus time the player gets in Active Camo while standing still.
Reduce the overall length of time the player can be in Active Camo.

Remove Sword block.
Disable the ability to block Sword attacks using melee with anything other than another Sword.
		*/
		public MegaloVariantTitleUpdateFlags Flags; // 0xF7A8 bitvector
		public float PrecisionWeaponBloom; // 0xF7AC
		public float ArmorLockEnergyBleedFromDamage; // 0xF7B0
		public float ArmorLockEnergyBleedFromDamageLimit; // 0xF7B4
		public float ActiveCamoEnergyBonus; // 0xF7B8
		public float ActiveCamoEnergy; // 0xF7BC
		public float MagnumProjectileDamage; // 0xF7C0
		public float MagnumFireRecoveryTime; // 0xF7C4

		public bool IsUnchanged { get {
			return Flags == 0 && PrecisionWeaponBloom == 1.0f && MagnumProjectileDamage == 1.0f && MagnumFireRecoveryTime == 1.0f &&
				ArmorLockEnergyBleedFromDamage == 0.0f && ArmorLockEnergyBleedFromDamageLimit == 0.0f &&
				ActiveCamoEnergyBonus == 0.02f &&
				ActiveCamoEnergy == 0.07f;
		} }

		public void Clear()
		{
			Flags = 0;
			PrecisionWeaponBloom = MagnumProjectileDamage = MagnumFireRecoveryTime = 1.0f;
			ArmorLockEnergyBleedFromDamage = ArmorLockEnergyBleedFromDamageLimit = 0.0f;
			ActiveCamoEnergyBonus = 0.02f;
			ActiveCamoEnergy = 0.07f;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			const int k_bit_length = 8;

			s.Stream(ref Flags, Bits.kInt32BitCount, MegaloVariantTitleUpdateFlagsBitStreamer.Instance);
			s.Stream(ref PrecisionWeaponBloom, 0.0f, 10.0f, k_bit_length, false, true);
			s.Stream(ref ArmorLockEnergyBleedFromDamage, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref ArmorLockEnergyBleedFromDamageLimit, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref ActiveCamoEnergyBonus, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref ActiveCamoEnergy, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref MagnumProjectileDamage, 0.0f, 10.0f, k_bit_length, false, true);
			s.Stream(ref MagnumFireRecoveryTime, 0.0f, 10.0f, k_bit_length, false, true);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags!=0, true);
			s.StreamAttributeOpt("PrecisionWeaponReticleBloom", ref PrecisionWeaponBloom, f=>f!=1.0f);

			if (s.StreamElementOpt("ArmorLockEnergyBleedFromDamage", ref ArmorLockEnergyBleedFromDamage, f => f != 0.0f))
				s.WriteComment("Damage received while in Armor Lock is transferred to remaining Armor Ability energy");
			if (s.StreamElementOpt("ArmorLockEnergyBleedFromDamageLimit", ref ArmorLockEnergyBleedFromDamageLimit, f => f != 0.0f))
				s.WriteComment("How much the damage depletes your Armor Lock energy");

			if (s.StreamElementOpt("ActiveCamoEnergyBonus", ref ActiveCamoEnergyBonus, f=>f!=0.02f))
				s.WriteComment("Bonus time the player gets in Active Camo while standing still");
			if (s.StreamElementOpt("ActiveCamoEnergy", ref ActiveCamoEnergy, f=>f!=0.07f))
				s.WriteComment("Overall length of time the player can be in Active Camo");

			s.StreamAttributeOpt("MagnumProjectileDamage", ref MagnumProjectileDamage, f=>f!=1.0f); // magnum bullet force and damage
			s.StreamAttributeOpt("MagnumFireRecoveryTime", ref MagnumFireRecoveryTime, f=>f!=1.0f);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameEngineMegaloVariantHaloReach
		: Blam.RuntimeData.Variants.GameEngineMegaloVariant
	{
		#region Constants
		//const int kSizeOf = 0x;
		const int kEncodingVersionStock = 0x6A;
		const int kEncodingVersionTU1 = 0x6B;

		public static LocaleStringTableInfo kStringTableInfo = new LocaleStringTableInfo(112, 0x4C00);
		public static LocaleStringTableInfo kNameStringTableInfo = new LocaleStringTableInfo(1, 0x180); // 32 chars per?
		public static LocaleStringTableInfo kDescriptionStringTableInfo = new LocaleStringTableInfo(1, 0xC00); // 256 chars per?
		public static LocaleStringTableInfo kCategoryStringTableInfo = kNameStringTableInfo;
		#endregion

		readonly GameEngineBaseVariantHaloReach mBaseVariant;
		public override Blam.RuntimeData.Variants.GameEngineBaseVariant BaseVariant { get { return mBaseVariant; } }

		bool unkF7A6, unkF7A7;

		public MegaloVariantTU1 TU1 { get; private set; }

		public GameEngineMegaloVariantHaloReach(Blam.RuntimeData.Variants.GameEngineVariant variantManager) : base(variantManager,
			kStringTableInfo,
			kNameStringTableInfo, kDescriptionStringTableInfo, kCategoryStringTableInfo)
		{
			mBaseVariant = new GameEngineBaseVariantHaloReach(variantManager);

			TU1 = new MegaloVariantTU1();
		}

		public override void ClearWeaponTunings()
		{
			TU1.Clear();
		}

		protected override bool VerifyEncodingVersion()
		{
			return mEncodingVersion >= kEncodingVersionStock && mEncodingVersion <= kEncodingVersionTU1;
		}

		#region IBitStreamSerializable Members
		protected override void SerializeDescriptionLocaleStrings(IO.BitStream s)
		{
			s.StreamObject(NameString);					// 0x
			s.StreamObject(DescriptionString);			// 0x
			s.StreamObject(CategoryString);				// 0x
		}
		protected override void SerializeImpl(IO.BitStream s)
		{
			#region struct
			// 0x	BaseVariant
			// 0x734	StringTable
			// 0x5DBC	EncodingVersion
			// 0x5DC0	PlayerTraits
			// 0x60C4	UserDefinedOptions
			// 0x63A8	EngineDefinition
			// 0xE668	BaseNameStringIndex
			// 0x	EngineIconIndex
			// 0x	EngineCategory
			// 0x	MapPermissions
			// 0x	PlayerRatingParameters
			// 0x	ScoreToWinRound
			// 0xE66C	DisabledEngineOptions
			// 0xE70C	HiddenEngineOptions
			// 0xE7AC	DisabledUserOptions
			// 0xE7B0	HiddenUserOptions
			#endregion

			s.Stream(ref unkF7A6);
			s.Stream(ref unkF7A7);
			SerializeOptionToggles(s);
			s.StreamObject(EngineDefinition);			// 0x63A8

			if (mEncodingVersion >= kEncodingVersionTU1)
				s.StreamObject(TU1);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		protected override void SerializeImpl<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.SerializeImpl(s);

			s.StreamAttribute("unkF7A6", ref unkF7A6);
			s.StreamAttribute("unkF7A7", ref unkF7A7);

			if (mEncodingVersion >= kEncodingVersionTU1)
			{
				using (var bm = s.EnterCursorBookmarkOpt("TU1", TU1, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
					s.StreamObject(TU1);
			}
		}
		#endregion
	};
}