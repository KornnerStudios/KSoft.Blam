#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public class GameEngineCampaignVariant
		: IGameEngineVariant
	{
		public GameEngineBaseVariant BaseVariant { get; private set; }

		protected GameEngineCampaignVariant(GameEngineVariant variantManager)
		{
			BaseVariant = GameEngineBaseVariant.Create(variantManager);
		}

		internal static GameEngineCampaignVariant Create(GameEngineVariant variantManager)
		{
			Contract.Requires(variantManager != null);

			var game_build = variantManager.GameBuild;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new GameEngineCampaignVariant(variantManager);

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
				return new GameEngineCampaignVariant(variantManager);

			throw new KSoft.Debug.UnreachableException(game_build.ToDisplayString());
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