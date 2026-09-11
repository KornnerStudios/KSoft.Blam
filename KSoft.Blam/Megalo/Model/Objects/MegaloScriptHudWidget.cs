
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
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mPosition), AlwaysNotify = true)]
		public partial MegaloScriptWidgetPosition Position { get; set; }
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