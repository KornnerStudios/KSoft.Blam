using System;

namespace KSoft.Blam.RuntimeData.Variants
{
	public enum PlayerRatingParameter
	{
		RatingScale,
		KillWeight,
		AssistWeight,
		BetrayalWeight,
		DeathWeight,
		NormalizeByMaxKills,
		Base,
		Range,
		LossScalar,
		CustomStat0,
		CustomStat1,
		CustomStat2,
		CustomStat3,
		Expansion0,
		Expansion1,

		kNumberOf
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloVariantPlayerRatingParameters
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public float[] ParameterArray { get; private set; }
		public bool ShowInScoreboard;

		public MegaloVariantPlayerRatingParameters()
		{
			ParameterArray = new float[(int)PlayerRatingParameter.kNumberOf];
			RevertToDefault();
		}

		public bool IsDefault { get {
			bool is_default = ShowInScoreboard==false;

			if (is_default)
			{
				for (int x = 0; x < ParameterArray.Length && is_default; x++)
				{
					var param = (PlayerRatingParameter)x;
					is_default = param switch
					{
						PlayerRatingParameter.RatingScale or
						PlayerRatingParameter.KillWeight or
						PlayerRatingParameter.AssistWeight or
						PlayerRatingParameter.BetrayalWeight or
						PlayerRatingParameter.NormalizeByMaxKills
							=> ParameterArray[x] == 1.0f,

						PlayerRatingParameter.DeathWeight
							=> ParameterArray[x] == 0.33f,

						PlayerRatingParameter.Base or
						PlayerRatingParameter.Range
							=> ParameterArray[x] == 1000.0f,

						PlayerRatingParameter.LossScalar
							=> ParameterArray[x] == 0.96f,

						_ => ParameterArray[x] == 0.0f,
					};
				}
			}

			return is_default;
		} }

		public void RevertToDefault()
		{
			System.Array.Clear(ParameterArray, 0, ParameterArray.Length);
			ParameterArray[(int)PlayerRatingParameter.RatingScale] = 1.0f;
			ParameterArray[(int)PlayerRatingParameter.KillWeight] = 1.0f;
			ParameterArray[(int)PlayerRatingParameter.AssistWeight] = 1.0f;
			ParameterArray[(int)PlayerRatingParameter.BetrayalWeight] = 1.0f;
			ParameterArray[(int)PlayerRatingParameter.DeathWeight] = 0.33f;
			ParameterArray[(int)PlayerRatingParameter.NormalizeByMaxKills] = 1.0f;
			ParameterArray[(int)PlayerRatingParameter.Base] = 1000.0f;
			ParameterArray[(int)PlayerRatingParameter.Range] = 1000.0f;
			ParameterArray[(int)PlayerRatingParameter.LossScalar] = 0.96f;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.StreamFixedArray(ParameterArray.AsSpan());
			s.Stream(ref ShowInScoreboard);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("showInScoreboard", ref ShowInScoreboard);

			int streamed_count = s.StreamFixedArray("Param", ParameterArray.AsSpan());

			Util.MarkUnusedVariable(ref streamed_count);
		}
		#endregion
	};
}
