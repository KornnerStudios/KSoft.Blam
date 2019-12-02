
namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsRepawningHaloReach
		: Blam.RuntimeData.Variants.GameOptionsRepawning
	{
		internal GameOptionsRepawningHaloReach(GameEngineBaseVariantHaloReach variant) : base(variant)
		{
			RevertToDefault();

			// #TODO_BLAM: default TraitsDuration
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeFlags(s, 4);
			s.Stream(ref LivesPerRound, 6);
			s.Stream(ref TeamLivesPerRound, 7);
			s.Stream(ref RespawnTime);
			s.Stream(ref SuicideTime);
			s.Stream(ref BetrayalTime);
			s.Stream(ref RespawnGrowthTime, 4);
			s.Stream(ref InitialLoadoutSelectionTime, 4);
			s.Stream(ref TraitsDuration, 6);
			s.StreamObject(Traits);
		}
		#endregion
	};
}