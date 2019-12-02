
namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsMiscHaloReach
		: Blam.RuntimeData.Variants.GameOptionsMisc
	{
		public byte GracePeriod;

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			base.SerializeFlags(s);
			s.Stream(ref RoundTimeLimit);
			s.Stream(ref RoundLimit, 5);
			s.Stream(ref EarlyVictoryWinCount, 4);
			s.StreamNoneable(ref SuddenDeathTimeLimit, 7);
			s.Stream(ref GracePeriod, 5);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOpt("gracePeriod", ref GracePeriod, Predicates.IsNotZero);
		}
		#endregion
	};
}