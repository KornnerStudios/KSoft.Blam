using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameEngineSandboxVariantFlagsBitStreamer = IO.EnumBitStreamer<GameEngineSandboxVariantFlags>;
	using SandboxEditingModeBitStreamer = IO.EnumBitStreamer<SandboxEditingMode>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameEngineSandboxVariantFlags : byte // bits are individually streamed in the engine
	{
		OpenChannelVoiceEnabled = 1<<0, // sandbox-open-channel-voice
		RequiresAllObjects = 1<<1,
	};
	public enum SandboxEditingMode : sbyte
	{
		AllPlayers,
		LeaderOnly,

		kNumberOf
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameEngineSandboxVariant
		: IGameEngineVariant
	{
		public GameEngineMegaloVariant MegaloVariant { get; private set; }
		public GameEngineSandboxVariantFlags Flags;
		public SandboxEditingMode EditMode;
		public short RespawnTime; // 0...60
		public PlayerTraitsBase EditorTraits { get; private set; }

		protected GameEngineSandboxVariant(GameEngineVariant variantManager)
		{
			MegaloVariant = GameEngineMegaloVariant.Create(variantManager);

			EditorTraits = MegaloVariant.BaseVariant.NewPlayerTraits();

			RevertToDefault();
		}

		public void RevertToDefault()
		{
			Flags |= GameEngineSandboxVariantFlags.OpenChannelVoiceEnabled;
			EditMode = SandboxEditingMode.AllPlayers;
			RespawnTime = TypeExtensionsBlam.kUsualDefaultRespawnTimeInSeconds;
			// #TODO_BLAM: EditorTraits

			MegaloVariant.BaseVariant.OptionsMisc.RoundLimit = 1;
			MegaloVariant.BaseVariant.OptionsMisc.RoundTimeLimit = 0;
			MegaloVariant.BaseVariant.OptionsMisc.Flags |= GameOptionsMiscFlags.TeamsEnabled;
			MegaloVariant.BaseVariant.OptionsRespawning.InitialLoadoutSelectionTime = 0;
		}

		internal static GameEngineSandboxVariant Create(GameEngineVariant variantManager)
		{
			Contract.Requires(variantManager != null);

			var game_build = variantManager.GameBuild;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.HaloReach.RuntimeData.Variants.GameEngineSandboxVariantHaloReach(variantManager);

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
				return new Games.Halo4.RuntimeData.Variants.GameEngineSandboxVariantHalo4(variantManager);

			throw new KSoft.Debug.UnreachableException(game_build.ToDisplayString());
		}

		#region IBitStreamSerializable Members
		protected virtual void SerializeImpl(IO.BitStream s)
		{
			MegaloVariant.Serialize(s);
			s.Stream(ref Flags, 2, GameEngineSandboxVariantFlagsBitStreamer.Instance);
			s.Stream(ref EditMode, 1, SandboxEditingModeBitStreamer.Instance);
			s.Stream(ref RespawnTime, 6);
			EditorTraits.Serialize(s);
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
			using (s.EnterCursorBookmark("Sandbox"))
			{
				s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != GameEngineSandboxVariantFlags.OpenChannelVoiceEnabled, true);
				s.StreamAttributeEnumOpt("editMode", ref EditMode, e => e != SandboxEditingMode.AllPlayers);
				s.StreamAttributeEnumOpt("respawnTime", ref RespawnTime, v => v!=TypeExtensionsBlam.kUsualDefaultRespawnTimeInSeconds);

				using (var bm = s.EnterCursorBookmarkOpt("EditorTraits", EditorTraits, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
					EditorTraits.Serialize(s);
			}
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			MegaloVariant.Serialize(s);

			using (s.EnterOwnerBookmark(this))
				SerializeImpl(s);
		}
		#endregion

		GameEngineType IGameEngineVariant.EngineType { get { return GameEngineType.Sandbox; } }
		GameEngineBaseVariant IGameEngineVariant.BaseVariant { get { return MegaloVariant.BaseVariant; } }
	};
}