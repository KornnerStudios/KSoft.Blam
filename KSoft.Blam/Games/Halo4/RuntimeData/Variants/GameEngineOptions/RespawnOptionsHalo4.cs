
namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsRepawningHalo4
		: Blam.RuntimeData.Variants.GameOptionsRepawning
	{
		public byte MinRespawnTime;

		public GameOptionsRepawningHalo4(GameEngineBaseVariantHalo4 variant) : base(variant)
		{
			RevertToDefault();
		}

		public override void RevertToDefault()
		{
			base.RevertToDefault();

			RespawnTime = 3;
			MinRespawnTime = 0;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeFlags(s, 5);
			s.Stream(ref LivesPerRound, 6);
			s.Stream(ref TeamLivesPerRound, 7);
			s.Stream(ref MinRespawnTime);
			s.Stream(ref RespawnTime);
			s.Stream(ref SuicideTime);
			s.Stream(ref BetrayalTime);
			s.Stream(ref RespawnGrowthTime, 4);
			s.Stream(ref InitialLoadoutSelectionTime, 4);
			s.Stream(ref TraitsDuration, 6);
			s.StreamObject(Traits);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeTimes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.SerializeTimes(s);

			s.StreamAttributeOpt("minRespawn", ref MinRespawnTime, Predicates.IsNotZero);
		}
		#endregion
	};
}