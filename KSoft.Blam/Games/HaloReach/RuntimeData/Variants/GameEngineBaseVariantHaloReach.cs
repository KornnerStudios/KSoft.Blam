
namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	sealed partial class GameEngineBaseVariantHaloReach
		: Blam.RuntimeData.Variants.GameEngineBaseVariant
	{
		public GameEngineBaseVariantHaloReach(Blam.RuntimeData.Variants.GameEngineVariant variantManager) : base(variantManager)
		{
			OptionsMisc = new GameOptionsMiscHaloReach();
			OptionsRespawning = new GameOptionsRepawningHaloReach(this);

			OptionsMapOverrides = new GameOptionsMapOverridesHaloReach(this);
			TeamOptions = new GameOptionsTeamOptionsHaloReach(variantManager.GameBuild);
			LoadoutOptions = new GameOptionsLoadoutsHaloReach();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.StreamObject(Header);									// 0x4
			SerializeFlags(s, 1);									// 0x72E
			s.StreamObject(OptionsMisc);							// 0x2B4
			s.StreamObject(OptionsRespawning);						// 0x2BC
			s.StreamObject(OptionsSocial);							// 0x2F6
			s.StreamObject(OptionsMapOverrides);					// 0x2F8
			s.StreamObject(TeamOptions);							// 0x3B4
			s.StreamObject(LoadoutOptions);							// 0x678
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);

			SerializeContentHeader(s);
			SerializeMiscOptions(s);
			SerializRespawnOptions(s);

			SerializeSocialOptions(s);
			SerializMapOverrides(s);

			SerializeTeams(s);

			SerializeLoadoutOptions(s);
		}
		#endregion
	};
}