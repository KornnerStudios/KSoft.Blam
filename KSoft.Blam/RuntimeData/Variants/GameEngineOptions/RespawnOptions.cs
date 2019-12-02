using System;

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameOptionsRepawningFlagsBitStreamer = IO.EnumBitStreamerWithOptions
		< GameOptionsRepawningFlags
		, IO.EnumBitStreamerOptions.ShouldBitSwap
		>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsRepawningFlags : byte // bits are individually streamed in the engine
	{
		SynchronizeWithTeam = 1<<0, // respawn-options-inherit-respawn-time
		Unknown1 = 1<<1, // respawn-options-respawn-with-teammate
		Unknown2 = 1<<2, // respawn-options-respawn-at-location
		RespawnOnKill = 1<<3, // teammates respawns on kills
		InstantRespawnEnabled = 1<<4, // Halo4
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsRepawning
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsRepawningFlags Flags;
		public byte LivesPerRound, TeamLivesPerRound,
			RespawnTime = 5, SuicideTime, BetrayalTime,
			RespawnGrowthTime, InitialLoadoutSelectionTime = 10,
			TraitsDuration;

		public PlayerTraitsBase Traits { get; private set; }

		bool LivesAreDefault { get {
			return LivesPerRound == 0 && TeamLivesPerRound == 0;
		} }
		protected virtual bool TimesAreDefault { get {
			return RespawnTime == 5 && SuicideTime == 5 && BetrayalTime == 5 && RespawnGrowthTime == 0 &&
				InitialLoadoutSelectionTime == 10;
		} }
		bool TraitsAreDefault { get {
			return TraitsDuration == 5 && Traits.IsUnchanged;
		} }
		public virtual bool IsDefault { get {
			return Flags == 0 && LivesAreDefault && TimesAreDefault;
		} }

		protected GameOptionsRepawning(GameEngineBaseVariant variant)
		{
			Traits = variant.NewPlayerTraits();
		}

		public virtual void RevertToDefault()
		{
			Flags = 0;
			LivesPerRound = TeamLivesPerRound = 0;
			RespawnTime = 5;
			SuicideTime = BetrayalTime =
				TraitsDuration = 5;
			RespawnGrowthTime = 0;
			InitialLoadoutSelectionTime = 10;
		}

		#region IBitStreamSerializable Members
		protected void SerializeFlags(IO.BitStream s, int flagsBitLength)
		{
			s.Stream(ref Flags, flagsBitLength, GameOptionsRepawningFlagsBitStreamer.Instance);
		}
		public abstract void Serialize(IO.BitStream s);
		#endregion
		#region ITagElementStringNameStreamable Members
		protected virtual void SerializeTimes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("respawn", ref RespawnTime/*, v=>v!=5*/); // #HACK: default respawn times differ in Reach and H4, so leaving predicate out for now
			s.StreamAttributeOpt("suicide", ref SuicideTime, v=>v!=5);
			s.StreamAttributeOpt("betrayal", ref BetrayalTime, v=>v!=5);
			s.StreamAttributeOpt("respawnGrowth", ref RespawnGrowthTime, Predicates.IsNotZero);
			s.StreamAttributeOpt("initialLoadoutSelection", ref InitialLoadoutSelectionTime, v=>v!=10);
		}
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, f => f != 0, true);
			using (var bm = s.EnterCursorBookmarkOpt("Lives", this, obj=>!obj.LivesAreDefault)) if (bm.IsNotNull)
			{
				s.StreamAttribute("perRound", ref LivesPerRound);
				s.StreamAttribute("perRoundTeam", ref TeamLivesPerRound);
			}
			using (s.EnterCursorBookmark("Times"))
				SerializeTimes(s);

			using (var bm = s.EnterCursorBookmarkOpt("Traits", Traits, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOpt("duration", ref TraitsDuration, Predicates.IsNotZero);
				s.StreamObject(Traits);
			}
		}
		#endregion
	};
}