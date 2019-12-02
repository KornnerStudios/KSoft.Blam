#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Games.Halo4.RuntimeData
{
	using GameDifficultyBitStreamer = IO.EnumBitStreamer<Blam.RuntimeData.GameDifficulty>;
	using MetagameScoringBitStreamer = IO.EnumBitStreamer<Blam.RuntimeData.MetagameScoring>;

	using GameDifficultyBinaryStreamer = IO.EnumBinaryStreamer<Blam.RuntimeData.GameDifficulty>;
	using MetagameScoringBinaryStreamer = IO.EnumBinaryStreamer<Blam.RuntimeData.MetagameScoring>;

	class ContentHeaderHalo4
		: Blam.RuntimeData.ContentHeader
	{
		public ContentHeaderHalo4(Engine.EngineBuildHandle h4Build) : base(h4Build)
		{
		}

		#region IBitStreamSerializable Members
		protected override void SerializeActivity(IO.BitStream s)
		{
			s.Stream(ref Activity, 2);
		}

		protected override void SerializeGameSpecificData(IO.BitStream s)
		{
			if (EngineType == Blam.RuntimeData.GameEngineType.Campaign)
			{
				s.Stream(ref CampaignId);
				s.Stream(ref DifficultyLevel, 2, GameDifficultyBitStreamer.Instance);
				s.Stream(ref GameScoring, 2, MetagameScoringBitStreamer.Instance);
				s.Stream(ref InsertionPoint); // has to be less than 12
				s.Stream(ref unk2A4);
			}
			else if (EngineType == Blam.RuntimeData.GameEngineType.Survival)
			{
				s.Stream(ref DifficultyLevel, 2, GameDifficultyBitStreamer.Instance);
				s.Stream(ref unk2A4);
			}
		}
		#endregion

		#region IEndianStreamSerializable Members
		protected override void SerializeGameSpecificData(IO.EndianStream s)
		{
			if (EngineType == Blam.RuntimeData.GameEngineType.Campaign)
			{
				s.Stream(ref CampaignId);
				s.Stream(ref DifficultyLevel, GameDifficultyBinaryStreamer.Instance);
				s.Stream(ref GameScoring, MetagameScoringBinaryStreamer.Instance);
				s.Stream(ref InsertionPoint);
				s.Stream(ref unk2A4);
			}
			else if (EngineType == Blam.RuntimeData.GameEngineType.Survival)
			{
				s.Stream(ref DifficultyLevel, GameDifficultyBinaryStreamer.Instance);
				s.Pad24();
				s.Stream(ref unk2A4);
			}
			else s.Pad64();

			s.Pad64();
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		protected override void SerializeActivity<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attrName, ref sbyte value)
		{
			var activity = s.IsReading ? GameActivity.None : (GameActivity)value;
			s.StreamAttributeEnum(attrName, ref activity);

			Contract.Assert(activity < GameActivity.kNumberOf);

			if (s.IsReading)
				value = (sbyte)activity;
		}
		protected override void SerializeGameSpecificData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			if (EngineType == Blam.RuntimeData.GameEngineType.Campaign)
			{
				using (s.EnterCursorBookmark("Campaign"))
				{
					s.StreamAttribute("id", ref CampaignId);
					s.StreamAttributeEnum("difficulty", ref DifficultyLevel);
					s.StreamAttribute("mapID_", ref unk2A4);
					s.StreamAttributeOpt("insertionPoint", ref InsertionPoint, Predicates.IsNotZero);
					s.StreamAttributeEnumOpt("scoring", ref GameScoring, e=>e!=Blam.RuntimeData.MetagameScoring.None);
				}
			}
			else if (EngineType == Blam.RuntimeData.GameEngineType.Survival)
			{
				using (s.EnterCursorBookmark("Survival"))
				{
					s.StreamAttributeEnum("difficulty", ref DifficultyLevel);
					s.StreamAttribute("mapID_", ref unk2A4);
				}
			}
		}
		#endregion
	};
}