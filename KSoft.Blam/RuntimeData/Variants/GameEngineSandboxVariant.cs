using System;

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameEngineSandboxVariantFlagsBitStreamer = IO.EnumBitStreamer<GameEngineSandboxVariantFlags>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameEngineSandboxVariantFlags : byte // bits are individually streamed in the engine
	{
		OpenChannelVoiceEnabled = 1<<0, // sandbox-open-channel-voice
		Unknown1 = 1<<1,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameEngineSandboxVariant
		: IGameEngineVariant
	{
		public GameEngineMegaloVariant MegaloVariant { get; private set; }
		public GameEngineSandboxVariantFlags Flags;
		public byte EditMode; // 0...2
		public short RespawnTime; // 0...60
		public PlayerTraitsBase EditorTraits { get; private set; }

		protected GameEngineSandboxVariant(Engine.EngineBuildHandle gameBuild)
		{
			MegaloVariant = GameEngineMegaloVariant.Create(gameBuild);

			EditorTraits = MegaloVariant.BaseVariant.NewPlayerTraits();
		}

		internal static GameEngineSandboxVariant Create(Engine.EngineBuildHandle gameBuild)
		{
#if false // TODO
			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.HaloReach.RuntimeData.Variants.GameEngineSandboxVariantHaloReach();
			else if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.Halo4.RuntimeData.Variants.GameEngineSandboxVariantHalo4();
			else
#endif
				throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
		}

		#region IBitStreamSerializable Members
		protected virtual void SerializeImpl(IO.BitStream s)
		{
			MegaloVariant.Serialize(s);
			s.Stream(ref Flags, 2, GameEngineSandboxVariantFlagsBitStreamer.Instance);
			s.Stream(ref EditMode, 2);
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
				s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);
				s.StreamAttribute("editMode", ref EditMode);
				s.StreamAttribute("respawnTime", ref RespawnTime);

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