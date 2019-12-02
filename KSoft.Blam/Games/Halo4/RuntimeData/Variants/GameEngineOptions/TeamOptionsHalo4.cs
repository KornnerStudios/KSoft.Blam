
namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	using LocaleStringTableInfo = Localization.StringTables.LocaleStringTableInfo;

	[System.Reflection.Obfuscation(Exclude=false)]
	public struct GameOptionsSingleTeamEmblemInfo
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public byte ForegroundIndex, BackgroundIndex, Flags,
				PrimaryColor, SecondaryColor, BackgroundColor;

		public bool IsDefault { get {
			return ForegroundIndex == 0 && BackgroundIndex == 0 && Flags == 0 &&
				PrimaryColor == 1 && SecondaryColor == 1 && BackgroundColor == 1;
		} }

		public void RevertToDefault()
		{
			ForegroundIndex = BackgroundIndex = Flags = 0;
			PrimaryColor = SecondaryColor = BackgroundColor = 1;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref ForegroundIndex);
			s.Stream(ref BackgroundIndex);
			s.Stream(ref Flags, 3); // ?, toggle background, toggle foreground
			s.Stream(ref PrimaryColor, 6);
			s.Stream(ref SecondaryColor, 6);
			s.Stream(ref BackgroundColor, 6);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("foreground", ref ForegroundIndex);
			s.StreamAttribute("background", ref BackgroundIndex);
			s.StreamAttribute("primaryColor", ref PrimaryColor);
			s.StreamAttribute("secondaryColor", ref SecondaryColor);
			s.StreamAttribute("backgroundColor", ref BackgroundColor);
			s.StreamAttributeOpt("flags", ref Flags, Predicates.IsNotZero);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsSingleTeamOptionsHalo4
		: Blam.RuntimeData.Variants.GameOptionsSingleTeamOptions
	{
		public static LocaleStringTableInfo kNameStringTableInfo = new LocaleStringTableInfo(1, 0x220);

		public uint InterfaceColorOverride;

		public GameOptionsSingleTeamEmblemInfo EmblemInfo;

		public override bool OverridesAreDefault { get {
			return base.OverridesAreDefault && InterfaceColorOverride == uint.MaxValue;
		} }

		public override bool IsDefault  { get {
			return EmblemInfo.IsDefault && base.IsDefault;
		} }

		public GameOptionsSingleTeamOptionsHalo4(Engine.EngineBuildHandle h4Build)
			: base(h4Build, kNameStringTableInfo)
		{
			RevertToDefault();
		}

		public override void RevertToDefault()
		{
			base.RevertToDefault();

			InterfaceColorOverride = uint.MaxValue;

			EmblemInfo.RevertToDefault();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeFlags(s, 6);
			s.StreamObject(NameString);
			s.StreamNoneable(ref InitialDesignator, 4);
			SerializeModelOverride(s);
			s.Stream(ref PrimaryColorOverride);
			s.Stream(ref SecondaryColorOverride);
			s.Stream(ref TextColorOverride);
			s.Stream(ref InterfaceColorOverride);
			s.Stream(ref FireteamCount, 5);
			s.StreamValue(ref EmblemInfo);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeOverrides<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.SerializeOverrides(s);

			s.StreamAttributeOpt("interfaceColor", ref InterfaceColorOverride, v=>v!=uint.MaxValue, NumeralBase.Hex);
		}
		protected override void SerializeImpl<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.SerializeImpl(s);

			using (var bm = s.EnterCursorBookmarkOpt("Emblem", EmblemInfo, obj=>!obj.IsDefault)) if (bm.IsNotNull)
				s.StreamValue(ref EmblemInfo);
			else if (s.IsReading)
				EmblemInfo.RevertToDefault();
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsTeamOptionsHalo4
		: Blam.RuntimeData.Variants.GameOptionsTeamOptions
	{
		readonly Engine.EngineBuildHandle mHalo4Build;

		readonly GameOptionsSingleTeamOptionsHalo4[] mTeams;
		public override Blam.RuntimeData.Variants.GameOptionsSingleTeamOptions[] Teams { get { return mTeams; } }

		public GameOptionsTeamOptionsHalo4(Engine.EngineBuildHandle h4Build)
		{
			mHalo4Build = h4Build;
			mTeams = new GameOptionsSingleTeamOptionsHalo4[8];

			for (int x = 0; x < Teams.Length; x++)
				Teams[x] = new GameOptionsSingleTeamOptionsHalo4(h4Build);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeModelOverride(s);
			s.Stream(ref DesignatorSwitchType, 2);
			foreach (var opt in mTeams) s.StreamObject(opt);	// 0xD04
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void SerializeTeams<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			int streamed_count = s.StreamableFixedArray("Team", mTeams,
				mHalo4Build, _h4Build => new GameOptionsSingleTeamOptionsHalo4(_h4Build));

			if (s.IsReading)
				for (; streamed_count < Teams.Length; streamed_count++)
					Teams[streamed_count].RevertToDefault();
		}
		#endregion
	};
}