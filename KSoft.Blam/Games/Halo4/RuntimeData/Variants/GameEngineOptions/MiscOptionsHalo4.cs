
namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsMiscHalo4
		: Blam.RuntimeData.Variants.GameOptionsMisc
	{
		public bool KillCamEnabled, PointsSystemEnabled,
			FinalKillCamEnabled;

		public bool unk0_bit4, unk0_bit5;
		public byte MoshDifficulty = 2;

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			base.SerializeFlags(s);
			s.Stream(ref RoundTimeLimit);
			s.Stream(ref RoundLimit, 5);
			s.Stream(ref EarlyVictoryWinCount, 4);
			s.Stream(ref KillCamEnabled);
			s.Stream(ref PointsSystemEnabled);
			s.Stream(ref FinalKillCamEnabled);
			s.Stream(ref SuddenDeathTimeLimit, 8);
			s.Stream(ref unk0_bit4);
			s.Stream(ref unk0_bit5);
			s.Stream(ref MoshDifficulty, 2);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOpt("killCamEnabled", ref KillCamEnabled, Predicates.IsTrue);
			s.StreamAttributeOpt("pointsSystemEnabled", ref PointsSystemEnabled, Predicates.IsTrue);
			s.StreamAttributeOpt("finalKillCamEnabled", ref FinalKillCamEnabled, Predicates.IsTrue);
			s.StreamAttributeOpt("bit4", ref unk0_bit4, Predicates.IsTrue);
			s.StreamAttributeOpt("bit5", ref unk0_bit5, Predicates.IsTrue);
			s.StreamAttributeOpt("moshDifficulty", ref MoshDifficulty, v=>v!=2);
		}
		#endregion
	};
}