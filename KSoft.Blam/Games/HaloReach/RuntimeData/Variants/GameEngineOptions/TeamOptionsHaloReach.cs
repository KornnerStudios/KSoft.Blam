
namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	using LocaleStringTableInfo = Localization.StringTables.LocaleStringTableInfo;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsSingleTeamOptionsHaloReach
		: Blam.RuntimeData.Variants.GameOptionsSingleTeamOptions
	{
		// max size: 0x20, but size bit length would suggest 0x40
		public static readonly LocaleStringTableInfo kNameStringTableInfo = new LocaleStringTableInfo(1, 0x20)
			.SetBufferRelatedBitLengths(5, 6);

		public GameOptionsSingleTeamOptionsHaloReach(Engine.EngineBuildHandle reachBuild)
			: base(reachBuild, kNameStringTableInfo)
		{
			RevertToDefault();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeFlags(s, 4);
			s.StreamObject(NameString);
			s.StreamNoneable(ref InitialDesignator, 4);
			SerializeModelOverride(s);
			s.Stream(ref PrimaryColorOverride);
			s.Stream(ref SecondaryColorOverride);
			s.Stream(ref TextColorOverride);
			s.Stream(ref FireteamCount, 5);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsTeamOptionsHaloReach
		: Blam.RuntimeData.Variants.GameOptionsTeamOptions
	{
		readonly Engine.EngineBuildHandle mReachBuild;

		public byte TeamScoringMethod; // I think? short at runtime

		readonly GameOptionsSingleTeamOptionsHaloReach[] mTeams;
		public override Blam.RuntimeData.Variants.GameOptionsSingleTeamOptions[] Teams { get { return mTeams; } }

		public override bool IsDefault  { get {
			return TeamScoringMethod == 0 && base.IsDefault;
		} }

		public GameOptionsTeamOptionsHaloReach(Engine.EngineBuildHandle reachBuild)
		{
			mReachBuild = reachBuild;
			mTeams = new GameOptionsSingleTeamOptionsHaloReach[8];

			for (int x = 0; x < Teams.Length; x++)
				mTeams[x] = new GameOptionsSingleTeamOptionsHaloReach(reachBuild);
		}

		public override void RevertToDefault()
		{
			base.RevertToDefault();

			TeamScoringMethod = 0;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.Stream(ref TeamScoringMethod, 3);				// 0x730

			SerializeModelOverride(s);
			s.Stream(ref DesignatorSwitchType, 2);
			foreach (var opt in mTeams) s.StreamObject(opt);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void SerializeTeams<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			int streamed_count = s.StreamableFixedArray("Team", mTeams,
				mReachBuild, _reachBuild => new GameOptionsSingleTeamOptionsHaloReach(_reachBuild));

			if (s.IsReading)
				for (; streamed_count < Teams.Length; streamed_count++)
					Teams[streamed_count].RevertToDefault();
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOpt("teamScoringMethod_", ref TeamScoringMethod, Predicates.IsNotZero);
		}
		#endregion
	};
}
