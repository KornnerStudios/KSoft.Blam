
namespace KSoft.Blam.RuntimeData.Variants
{
	using GameEngineTypeBitStreamer = IO.EnumBitStreamer<GameEngineType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public interface IGameEngineVariant
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		GameEngineType EngineType { get; }
		GameEngineBaseVariant BaseVariant { get; }
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameEngineVariant
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public Engine.EngineBuildHandle GameBuild;
		public GameEngineType Type = GameEngineType.None;
		public IGameEngineVariant Variant { get; private set; }

		public GameEngineVariant(Engine.EngineBuildHandle gameBuild)
		{
			GameBuild = gameBuild;
		}
		public GameEngineVariant(Engine.EngineBuildHandle gameBuild, GameEngineType type)
		{
			GameBuild = gameBuild;
			Type = type;
			InitializeVariant();
		}

		void InitializeVariant()
		{
			switch (Type)
			{
#if false // #TODO_BLAM_REFACTOR
				case GameEngineType.None:
					Variant = GameEngineBaseVariant.Create(GameBuild);
					break;
				case GameEngineType.Sandbox:
					Variant = GameEngineSandboxVariant.Create(GameBuild);
					break;
				case GameEngineType.Megalo:
					Variant = GameEngineMegaloVariant.Create(GameBuild);
					break;
				case GameEngineType.Campaign:
					Variant = GameEngineCampaignVariant.Create(GameBuild);
					break;
				case GameEngineType.Survival:
					throw new NotImplementedException(Type.ToString());
					//Variant = GameEngineSurvivalVariant.Create(GameBuild);
					//break;
				case GameEngineType.Firefight:
					throw new NotImplementedException(Type.ToString());
					//Variant = GameEngineFirefightVariant.Create(GameBuild);
					//break;
#endif
				default: throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Type, 4, GameEngineTypeBitStreamer.Instance);
			if (s.IsReading)
				InitializeVariant();

			if (Variant != null)
				Variant.Serialize(s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			Engine.EngineBuildHandle.Serialize(s, ref GameBuild);
			s.StreamAttributeEnum("engineType", ref Type);
			if (s.IsReading)
				InitializeVariant();

			if (Variant != null)
				Variant.Serialize(s);
		}
		#endregion

		public GameEngineMegaloVariant TryGetMegaloVariant()
		{
			if (Variant is GameEngineMegaloVariant)
				return (GameEngineMegaloVariant)Variant;
#if false // #TODO_BLAM_REFACTOR
			else if (Variant is GameEngineSandboxVariant)
				return ((GameEngineSandboxVariant)Variant).MegaloVariant;
#endif
			return null;
		}
	};
}