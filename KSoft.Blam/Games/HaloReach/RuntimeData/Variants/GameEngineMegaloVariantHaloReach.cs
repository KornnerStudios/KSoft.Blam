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
		MagnumIsAutomatic = 1<<5,

		#region Unused
		Unused6 = 1<<6,
		Unused7 = 1<<7,
		Unused8 = 1<<8,
		Unused9 = 1<<9,
		Unused10 = 1<<10,
		Unused11 = 1<<11,
		Unused12 = 1<<12,
		Unused13 = 1<<13,
		Unused14 = 1<<14,
		Unused15 = 1<<15,
		Unused16 = 1<<16,
		Unused17 = 1<<17,
		Unused18 = 1<<18,
		Unused19 = 1<<19,
		Unused20 = 1<<20,
		Unused21 = 1<<21,
		Unused22 = 1<<22,
		Unused23 = 1<<23,
		Unused24 = 1<<24,
		Unused25 = 1<<25,
		Unused26 = 1<<26,
		Unused27 = 1<<27,
		Unused28 = 1<<28,
		Unused29 = 1<<29,
		Unused30 = 1<<30,
		Unused31 = 1U<<31,
		#endregion
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
		public float ArmorLockDamageToEnergyTransfer; // 0xF7B0
		public float ArmorLockDamageToEnergyCap; // 0xF7B4
		public float ActiveCamoOverrideEnergyCurveMin; // 0xF7B8
		public float ActiveCamoOverrideEnergyCurveMax; // 0xF7BC
		public float MagnumDamageMultiplier; // 0xF7C0
		public float MagnumFireRecoveryTimeMultiplier; // 0xF7C4

		public bool IsUnchanged { get {
			return Flags == 0 && PrecisionWeaponBloom == 1.0f && MagnumDamageMultiplier == 1.0f && MagnumFireRecoveryTimeMultiplier == 1.0f &&
				ArmorLockDamageToEnergyTransfer == 0.0f && ArmorLockDamageToEnergyCap == 0.0f &&
				ActiveCamoOverrideEnergyCurveMin == 0.02f &&
				ActiveCamoOverrideEnergyCurveMax == 0.07f;
		} }

		public void Clear()
		{
			Flags = 0;
			PrecisionWeaponBloom = MagnumDamageMultiplier = MagnumFireRecoveryTimeMultiplier = 1.0f;
			ArmorLockDamageToEnergyTransfer = ArmorLockDamageToEnergyCap = 0.0f;
			ActiveCamoOverrideEnergyCurveMin = 0.02f;
			ActiveCamoOverrideEnergyCurveMax = 0.07f;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			const int k_bit_length = 8;

			s.Stream(ref Flags, Bits.kInt32BitCount, MegaloVariantTitleUpdateFlagsBitStreamer.Instance); // big_flags have their entire chunk written, instead of only the used flags
			s.Stream(ref PrecisionWeaponBloom, 0.0f, 10.0f, k_bit_length, false, true);
			s.Stream(ref ArmorLockDamageToEnergyTransfer, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref ArmorLockDamageToEnergyCap, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref ActiveCamoOverrideEnergyCurveMin, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref ActiveCamoOverrideEnergyCurveMax, 0.0f, 2.0f, k_bit_length, false, true);
			s.Stream(ref MagnumDamageMultiplier, 0.0f, 10.0f, k_bit_length, false, true);
			s.Stream(ref MagnumFireRecoveryTimeMultiplier, 0.0f, 10.0f, k_bit_length, false, true);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags!=0, true);
			s.StreamAttributeOpt("PrecisionWeaponReticuleBloom", ref PrecisionWeaponBloom, f=>f!=1.0f);

			if (s.StreamElementOpt("ArmorLockDamageToEnergyTransfer", ref ArmorLockDamageToEnergyTransfer, f => f != 0.0f))
				s.WriteComment("Damage received while in Armor Lock is transferred to remaining Armor Ability energy");
			s.StreamElementOpt("ArmorLockDamageToEnergyCap", ref ArmorLockDamageToEnergyCap, f => f != 0.0f);

			s.StreamElementOpt("ActiveCamoOverrideEnergyCurveMin", ref ActiveCamoOverrideEnergyCurveMin, f => f != 0.02f);
			s.StreamElementOpt("ActiveCamoOverrideEnergyCurveMax", ref ActiveCamoOverrideEnergyCurveMax, f => f != 0.07f);

			s.StreamAttributeOpt("MagnumDamageMultiplier", ref MagnumDamageMultiplier, f=>f!=1.0f);
			s.StreamAttributeOpt("MagnumFireRecoveryTimeMultiplier", ref MagnumFireRecoveryTimeMultiplier, f=>f!=1.0f);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameEngineMegaloVariantHaloReach
		: Blam.RuntimeData.Variants.GameEngineMegaloVariant
	{
		#region Constants
		//const int kSizeOf = 0x;
		public const int kEncodingVersionStock = 0x6A;
		public const int kEncodingVersionTU1 = 0x6B;

		public static readonly LocaleStringTableInfo kStringTableInfo = new LocaleStringTableInfo(112, 0x4C00);
		public static readonly LocaleStringTableInfo kNameStringTableInfo = new LocaleStringTableInfo(1, 0x180); // 32 chars per?
		public static readonly LocaleStringTableInfo kDescriptionStringTableInfo = new LocaleStringTableInfo(1, 0xC00); // 256 chars per?
		public static readonly LocaleStringTableInfo kCategoryStringTableInfo = kNameStringTableInfo;
		#endregion

		readonly GameEngineBaseVariantHaloReach mBaseVariant;
		public override Blam.RuntimeData.Variants.GameEngineBaseVariant BaseVariant => mBaseVariant;

		public bool FireTeamsEnabled, SymmetricGametype;

		public MegaloVariantTU1 TU1 { get; private set; }

		public GameEngineMegaloVariantHaloReach(Blam.RuntimeData.Variants.GameEngineVariant variantManager) : base(variantManager,
			kStringTableInfo,
			kNameStringTableInfo, kDescriptionStringTableInfo, kCategoryStringTableInfo)
		{
			mBaseVariant = new GameEngineBaseVariantHaloReach(variantManager);

			TU1 = new MegaloVariantTU1();
		}

		public override void ClearTitleUpdateData()
		{
			TU1.Clear();
		}

		protected override bool VerifyEncodingVersion() =>
			mEncodingVersion >= kEncodingVersionStock && mEncodingVersion <= kEncodingVersionTU1;

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

			s.Stream(ref FireTeamsEnabled);
			s.Stream(ref SymmetricGametype);
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

			s.StreamAttribute("fireTeamsEnabled", ref FireTeamsEnabled);
			s.StreamAttribute("symmetricGametype", ref SymmetricGametype);

			if (mEncodingVersion >= kEncodingVersionTU1)
			{
				using (var bm = s.EnterCursorBookmarkOpt("TU1", TU1, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
					s.StreamObject(TU1);
			}
		}
		#endregion
	};
}
