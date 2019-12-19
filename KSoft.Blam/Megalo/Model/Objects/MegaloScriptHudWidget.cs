
namespace KSoft.Blam.Megalo.Model
{
	using MegaloScriptWidgetPositionBitStreamer = IO.EnumBitStreamer<MegaloScriptWidgetPosition>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptHudWidget
		: MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region Position
		MegaloScriptWidgetPosition mPosition; // byte at runtime
		public MegaloScriptWidgetPosition Position {
			get { return mPosition; }
			set { mPosition = value;
				NotifyPropertyChanged(kPositionChanged);
		} }
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref mPosition, 4, MegaloScriptWidgetPositionBitStreamer.Instance);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("position", ref mPosition);

			SerializeCodeName(s);
		}
		#endregion
	};
}