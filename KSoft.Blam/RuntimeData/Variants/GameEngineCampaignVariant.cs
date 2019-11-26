
namespace KSoft.Blam.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public class GameEngineCampaignVariant
		: IGameEngineVariant
	{
		public GameEngineBaseVariant BaseVariant { get; private set; }

		protected GameEngineCampaignVariant(Engine.EngineBuildHandle gameBuild)
		{
			BaseVariant = GameEngineBaseVariant.Create(gameBuild);
		}

		internal static GameEngineCampaignVariant Create(Engine.EngineBuildHandle gameBuild)
		{
			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new GameEngineCampaignVariant(gameBuild);
			else if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new GameEngineCampaignVariant(gameBuild);
			else
				throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
		}

		#region IBitStreamSerializable Members
		protected virtual void SerializeImpl(IO.BitStream s)
		{
			BaseVariant.Serialize(s);
		}
		public void Serialize(IO.BitStream s)
		{
			using (s.EnterOwnerBookmark(this))
				SerializeImpl(s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		protected virtual void SerializeImpl<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Base"))
				s.StreamObject(BaseVariant);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterOwnerBookmark(this))
				SerializeImpl(s);
		}
		#endregion

		GameEngineType IGameEngineVariant.EngineType { get { return GameEngineType.Campaign; } }
	};
}