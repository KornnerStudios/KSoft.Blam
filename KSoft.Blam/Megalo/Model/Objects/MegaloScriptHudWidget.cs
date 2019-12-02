
namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptHudWidget
		: MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region Position
		int mPosition; // byte at runtime
		public int Position {
			get { return mPosition; }
			set { mPosition = value;
				NotifyPropertyChanged(kPositionChanged);
		} }
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref mPosition, 4);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("position", ref mPosition);

			SerializeCodeName(s);
		}
		#endregion
	};
}