
namespace KSoft.Blam.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloVariantPlayerRatingParameters
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public float[] unk1ACB0 { get; private set; }
		public bool Flag;

		public MegaloVariantPlayerRatingParameters()
		{
			unk1ACB0 = new float[15];
			for (int x = 0; x < unk1ACB0.Length; x++)
				unk1ACB0[x] = 1.0f;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.StreamFixedArray(unk1ACB0);
			s.Stream(ref Flag);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("flag", ref Flag);

			int streamed_count = s.StreamFixedArray("Param", unk1ACB0);

			if (s.IsReading)
				for (; streamed_count < unk1ACB0.Length; streamed_count++)
					unk1ACB0[streamed_count] = 1.0f;
		}
		#endregion
	};
}