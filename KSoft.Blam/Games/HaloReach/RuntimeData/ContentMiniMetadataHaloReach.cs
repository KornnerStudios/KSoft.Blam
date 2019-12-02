
namespace KSoft.Blam.Games.HaloReach.RuntimeData
{
	using GameActivityBitStreamer = IO.EnumBitStreamerWithOptions<GameActivity, IO.EnumBitStreamerOptions.ShouldUseNoneSentinelEncoding>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class ContentMiniMetadataHaloReach
		: Blam.RuntimeData.ContentMiniMetadata
	{
		public GameActivity Activity = GameActivity.None;

		public ContentMiniMetadataHaloReach(Engine.EngineBuildHandle buildHandle) : base(buildHandle)
		{
		}

		protected override void SerializeActivity(IO.BitStream s)
		{
			s.Stream(ref Activity, 3, GameActivityBitStreamer.Instance);
		}
		protected override void SerializeActivity<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnumOpt("activity", ref Activity, e => e != GameActivity.None);
		}
	};
}