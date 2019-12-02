
namespace KSoft.Blam.Games.Halo4.RuntimeData
{
	using GameActivityBitStreamer = IO.EnumBitStreamer<GameActivity>;

	public class ContentMiniMetadataHalo4
		: Blam.RuntimeData.ContentMiniMetadata
	{
		public GameActivity Activity;

		public ContentMiniMetadataHalo4(Engine.EngineBuildHandle h4Build) : base(h4Build)
		{
		}

		protected override void SerializeActivity(IO.BitStream s)
		{
			s.Stream(ref Activity, 2, GameActivityBitStreamer.Instance);
		}
		protected override void SerializeActivity<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnumOpt("activity", ref Activity, e => e != GameActivity.None);
		}
	};
}