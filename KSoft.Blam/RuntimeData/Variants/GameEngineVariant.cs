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
		public const string kGameVariantBinExtension = ".bin";
		public const string kGameVariantXmlExtension = ".gvarxml";

		Engine.EngineBuildHandle mGameBuild;
		GameEngineType mType = GameEngineType.None;

		public IGameEngineVariant Variant { get; private set; }

		public Engine.EngineBuildHandle GameBuild => mGameBuild;
		public GameEngineType Type => mType;

		#region EngineSystem references
		Engine.EngineSystemReference<Localization.LanguageSystem> mLanguageSystemRef =
			Engine.EngineSystemReference<Localization.LanguageSystem>.None;
		Engine.EngineSystemReference<Megalo.Proto.MegaloProtoSystem> mMegaloProtoSystemRef =
			Engine.EngineSystemReference<Megalo.Proto.MegaloProtoSystem>.None;

		public Localization.LanguageSystem LanguageSystem => mLanguageSystemRef;
		public Megalo.Proto.MegaloProtoSystem MegaloProtoSystem => mMegaloProtoSystemRef;

		bool RequiresMegaloProtoSystem
			=> Type == GameEngineType.Megalo || Type == GameEngineType.Sandbox;
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
			{
				mMegaloProtoSystemRef = engine.TryGetSystem<Megalo.Proto.MegaloProtoSystem>(GameBuild);
			}

			Variant = Type switch
			{
				GameEngineType.None => GameEngineBaseVariant.Create(this),
				GameEngineType.Sandbox => GameEngineSandboxVariant.Create(this),
				GameEngineType.Megalo => GameEngineMegaloVariant.Create(this),
				GameEngineType.Campaign => GameEngineCampaignVariant.Create(this),
				GameEngineType.Survival =>
					throw new NotImplementedException(Type.ToString()),
					//GameEngineSurvivalVariant.Create(this),
				GameEngineType.Firefight =>
					throw new NotImplementedException(Type.ToString()),
					//GameEngineFirefightVariant.Create(this),
				_ => throw new KSoft.Debug.UnreachableException(Type.ToString()),
			};
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref mType, 4, GameEngineTypeBitStreamer.Instance);
			if (s.IsReading)
			{
				InitializeVariant();
			}

#pragma warning disable IDE0031 // Use null propagation
			if (Variant != null)
			{
				Variant.Serialize(s);
			}
#pragma warning restore IDE0031 // Use null propagation
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
			{
				InitializeVariant();
			}

#pragma warning disable IDE0031 // Use null propagation
			if (Variant != null)
			{
				Variant.Serialize(s);
			}
#pragma warning restore IDE0031 // Use null propagation
		}
		#endregion

		public GameEngineMegaloVariant TryGetMegaloVariant()
		{
			if (!RequiresMegaloProtoSystem)
			{
				return null;
			}

			if (Variant is GameEngineMegaloVariant megaloVariant)
			{
				return megaloVariant;
			}

			if (Variant is GameEngineSandboxVariant sandboxVariant)
			{
				return sandboxVariant.MegaloVariant;
			}

			return null;
		}
	};
}
