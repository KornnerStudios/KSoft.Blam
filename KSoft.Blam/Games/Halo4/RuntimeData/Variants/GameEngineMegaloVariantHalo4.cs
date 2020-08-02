using System.Collections.Generic;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	using LocaleStringTableInfo = Localization.StringTables.LocaleStringTableInfo;
	using LocaleStringTable = Localization.StringTables.LocaleStringTable;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloVariantLoadout
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable // 0xB
	{
		public byte Size;
		public GameOptionsLoadoutHalo4 Loadout { get; private set; }

		public bool IsUsed => Loadout.IsUsed;

		public MegaloVariantLoadout()
		{
			Loadout = new GameOptionsLoadoutHalo4();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Size, 2);
			s.StreamObject(Loadout);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("size_", ref Size);
			s.StreamObject(Loadout);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class GameEngineMegaloVariantHalo4 : Blam.RuntimeData.Variants.GameEngineMegaloVariant
	{
		#region Constants
		public const int kSizeOf = 0x1AD40;
		public const int kSizeOfTU5 = 0x1C724;
		public const int kEncodingVersionStock = 0x103;

		public static readonly LocaleStringTableInfo kStringTableInfo = new LocaleStringTableInfo(148, 0xA000);
		public static readonly LocaleStringTableInfo kNameStringTableInfo = new LocaleStringTableInfo(1, 0x440); // 32 chars per?
		public static readonly LocaleStringTableInfo kDescriptionStringTableInfo = new LocaleStringTableInfo(1, 0x1100); // 256 chars per?
		public static readonly LocaleStringTableInfo kCategoryStringTableInfo = GameOptionsSingleTeamOptionsHalo4.kNameStringTableInfo;
		#endregion

		readonly GameEngineBaseVariantHalo4 mBaseVariant;
		public override Blam.RuntimeData.Variants.GameEngineBaseVariant BaseVariant { get { return mBaseVariant; } }

		public LocaleStringTable IntroDescriptionString { get; private set; }

		bool unk1ACF2, unk1ACF3, unk1ACF4;
		public List<MegaloVariantLoadout> Loadouts { get; private set; }
		public WeaponTuningData WeaponTuning;

		public bool HasWeaponTuning => mBaseVariant.OptionsPrototype.Mode == 1;

		public GameEngineMegaloVariantHalo4(Blam.RuntimeData.Variants.GameEngineVariant variantManager) : base(variantManager,
			kStringTableInfo,
			kNameStringTableInfo, kDescriptionStringTableInfo, kCategoryStringTableInfo)
		{
			var h4_build = variantManager.GameBuild;

			mBaseVariant = new GameEngineBaseVariantHalo4(variantManager);

			IntroDescriptionString = new LocaleStringTable(kDescriptionStringTableInfo, h4_build);

			Loadouts = new List<MegaloVariantLoadout>(MegaloDatabase.Limits.Loadouts.MaxCount);

			WeaponTuning = new WeaponTuningData();
		}

		public override void ClearWeaponTunings()
		{
			WeaponTuning.Clear();
		}

		protected override bool VerifyEncodingVersion()
		{
			return mEncodingVersion == kEncodingVersionStock;
		}

		#region IBitStreamSerializable Members
		protected override void SerializeDescriptionLocaleStrings(IO.BitStream s)
		{
			s.StreamObject(NameString);					// 0x18350
			s.StreamObject(DescriptionString);			// 0x187BC
			s.StreamObject(IntroDescriptionString);		// 0x198E8. haven't seen this used...
			s.StreamObject(CategoryString);				// 0x1AA14
		}
		protected override void SerializeImpl(IO.BitStream s)
		{
			#region struct
			// 0x								BaseVariant
			// 0x33EC							StringTable
			// 0xE79C							EncodingVersion
			// 0xE7A0, 0xE7A4, sizeof(0x104)	PlayerTraits
			// 0xF7E8, 0xFCA8, 0xF7E4;count		UserDefinedOptions
			// 0xFCC8							EngineDefinition
			// 0x18274							BaseNameStringIndex
			// 0x1AC64							EngineIconIndex
			// 0x1AC60							EngineCategory
			// 0x1AC68							MapPermissions
			// 0x1ACB0							PlayerRatingParameters
			// 0x1ACF0							ScoreToWinRound
			// 0x1ACF2
			// 0x1ACF3
			// 0x1ACF4
			// 0x1ACF8							Loadouts
			// 0x18278							DisabledEngineOptions
			// 0x182E0							HiddenEngineOptions
			// 0x18348							DisabledUserOptions
			// 0x1834C							HiddenUserOptions
			// 0x1AE44							TU5
			#endregion

			s.Stream(ref unk1ACF2);
			s.Stream(ref unk1ACF3);
			s.Stream(ref unk1ACF4);
			s.StreamElements(Loadouts, Bits.kInt32BitCount);
			SerializeOptionToggles(s);
			s.StreamObject(EngineDefinition);

			if (HasWeaponTuning)
			{
				WeaponTuning.Serialize(s);
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeLocaleStrings<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.SerializeLocaleStrings(s);

			using (var bm = s.EnterCursorBookmarkOpt("IntroString", IntroDescriptionString, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamObject(IntroDescriptionString);
		}
		protected override void SerializeImpl<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.SerializeImpl(s);

			using (var bm = s.EnterCursorBookmarkOpt("Loadouts", Loadouts, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamableElements("entry", Loadouts);

			s.StreamAttribute("unk1ACF2", ref unk1ACF2);
			s.StreamAttribute("unk1ACF3", ref unk1ACF3);
			s.StreamAttribute("unk1ACF4", ref unk1ACF4);

			if (HasWeaponTuning)
			{
				using (var bm = s.EnterCursorBookmarkOpt("WeaponTuning", WeaponTuning, obj=>!obj.IsUnchanged)) if(bm.IsNotNull)
					WeaponTuning.Serialize(s);
			}
		}
		#endregion
	};
}
