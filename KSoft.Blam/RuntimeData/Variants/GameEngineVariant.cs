using System;

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
		: IDisposable
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		Engine.EngineBuildHandle mGameBuild;
		GameEngineType mType = GameEngineType.None;

		public IGameEngineVariant Variant { get; private set; }

		public Engine.EngineBuildHandle GameBuild { get { return mGameBuild; } }
		public GameEngineType Type { get { return mType; } }

		#region EngineSystem references
		Engine.EngineSystemReference<Localization.LanguageSystem> mLanguageSystemRef =
			Engine.EngineSystemReference<Localization.LanguageSystem>.None;
		Engine.EngineSystemReference<Megalo.Proto.MegaloProtoSystem> mMegaloProtoSystemRef =
			Engine.EngineSystemReference<Megalo.Proto.MegaloProtoSystem>.None;

		public Localization.LanguageSystem LanguageSystem { get { return mLanguageSystemRef; } }
		public Megalo.Proto.MegaloProtoSystem MegaloProtoSystem { get { return mMegaloProtoSystemRef; } }

		bool RequiresMegaloProtoSystem { get {
			return Type == GameEngineType.Megalo || Type == GameEngineType.Sandbox;
		} }
		#endregion

		public GameEngineVariant(Engine.EngineBuildHandle gameBuild)
		{
			mGameBuild = gameBuild;
		}
		public GameEngineVariant(Engine.EngineBuildHandle gameBuild, GameEngineType type)
		{
			mGameBuild = gameBuild;
			mType = type;
			InitializeVariant();
		}

		#region IDisposable Members
		public void Dispose()
		{
			mLanguageSystemRef.Dispose();
			mMegaloProtoSystemRef.Dispose();
		}
		#endregion

		void InitializeVariant()
		{
			var engine = GameBuild.Engine;

			mLanguageSystemRef = engine.GetSystem<Localization.LanguageSystem>(GameBuild);

			if (RequiresMegaloProtoSystem)
				mMegaloProtoSystemRef = engine.TryGetSystem<Megalo.Proto.MegaloProtoSystem>(GameBuild);

			switch (Type)
			{
				case GameEngineType.None:
					Variant = GameEngineBaseVariant.Create(this);
					break;
				case GameEngineType.Sandbox:
					Variant = GameEngineSandboxVariant.Create(this);
					break;
				case GameEngineType.Megalo:
					Variant = GameEngineMegaloVariant.Create(this);
					break;
				case GameEngineType.Campaign:
					Variant = GameEngineCampaignVariant.Create(this);
					break;
				case GameEngineType.Survival:
					throw new NotImplementedException(Type.ToString());
					//Variant = GameEngineSurvivalVariant.Create(this);
					//break;
				case GameEngineType.Firefight:
					throw new NotImplementedException(Type.ToString());
					//Variant = GameEngineFirefightVariant.Create(this);
					//break;

				default: throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref mType, 4, GameEngineTypeBitStreamer.Instance);
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
			Engine.EngineBuildHandle.Serialize(s, ref mGameBuild);
			s.StreamAttributeEnum("engineType", ref mType);
			if (s.IsReading)
				InitializeVariant();

			if (Variant != null)
				Variant.Serialize(s);
		}
		#endregion

		public GameEngineMegaloVariant TryGetMegaloVariant()
		{
			if (!RequiresMegaloProtoSystem)
				return null;

			if (Variant is GameEngineMegaloVariant)
				return (GameEngineMegaloVariant)Variant;

			if (Variant is GameEngineSandboxVariant)
				return ((GameEngineSandboxVariant)Variant).MegaloVariant;

			return null;
		}
	};
}