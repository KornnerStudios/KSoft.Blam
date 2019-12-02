
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class SingleEncoding
		: IO.ITagElementStringNameStreamable
	{
		public string Name;
		public int BitLength;
		public float Min, Max;
		public bool IsSigned;
		public bool Flag1 = true;

		/// <summary>Is this encoding actually just an alias for a normal 32-bit float?</summary>
		public bool NoEncoding { get { return BitLength == Bits.kInt32BitCount; } }

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("bitLength", ref BitLength);
			if (!NoEncoding)
			{
				s.StreamAttribute("min", ref Min);
				s.StreamAttribute("max", ref Max);
				s.StreamAttributeOpt("isSigned", ref IsSigned, Predicates.IsTrue);
				s.StreamAttributeOpt("flag1", ref Flag1, Predicates.IsFalse);
			}
			s.StreamAttribute("name", ref Name);
		}
		#endregion
	};
}