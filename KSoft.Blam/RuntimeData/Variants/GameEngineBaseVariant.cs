using System;

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameEngineBaseVariantFlagsBitStreamer = IO.EnumBitStreamerWithOptions
		< GameEngineBaseVariantFlags
		, IO.EnumBitStreamerOptions.ShouldBitSwapWithOneBitGuard
		>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameEngineBaseVariantFlags : byte // bits are individually streamed in the engine
	{
		Unknown0 = 1<<0,
		Unknown1 = 1<<1, // Halo4
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class GameEngineBaseVariant : IGameEngineVariant
	{
		public GameEngineBaseVariantFlags Flags;

		public ContentHeader Header { get; private set; }

		public GameOptionsMisc OptionsMisc { get; protected set; }
		public GameOptionsRepawning OptionsRespawning { get; protected set; }

		public GameOptionsSocial OptionsSocial { get; protected set; }

		public GameOptionsMapOverrides OptionsMapOverrides { get; protected set; }

		public GameOptionsTeamOptions TeamOptions { get; protected set; }
		public GameOptionsLoadouts LoadoutOptions { get; protected set; }

		protected GameEngineBaseVariant(Engine.EngineBuildHandle gameBuild)
		{
			Header = ContentHeader.Create(gameBuild);

			OptionsSocial = new GameOptionsSocial();
		}

		internal static GameEngineBaseVariant Create(Engine.EngineBuildHandle gameBuild)
		{
#if false // #TODO
			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.HaloReach.RuntimeData.Variants.GameEngineBaseVariantHaloReach();
			else if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.Halo4.RuntimeData.Variants.GameEngineBaseVariantHalo4();
			else
#endif
				throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
		}

		#region IBitStreamSerializable Members
		protected void SerializeFlags(IO.BitStream s, int flagsBitLength)
		{
			s.Stream(ref Flags, flagsBitLength, GameEngineBaseVariantFlagsBitStreamer.Instance);
		}

		public abstract void Serialize(IO.BitStream s);
		#endregion

		#region ITagElementStringNameStreamable Members
		protected void SerializeContentHeader<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Header"))
				s.StreamObject(Header);
		}
		protected void SerializeMiscOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Misc"))
				s.StreamObject(OptionsMisc);
		}
		protected void SerializRespawnOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Respawning"))
				s.StreamObject(OptionsRespawning);
		}
		protected void SerializeSocialOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Social", OptionsSocial, opts=>!opts.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(OptionsSocial);
			else if (s.IsReading)
				OptionsSocial.RevertToDefault();
		}
		protected void SerializMapOverrides<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("MapOverrides"))
				s.StreamObject(OptionsMapOverrides);
		}
		protected void SerializeTeams<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Teams", TeamOptions, opts=>!opts.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(TeamOptions);
			else if (s.IsReading)
				TeamOptions.RevertToDefault();
		}
		protected void SerializeLoadoutOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Loadouts", LoadoutOptions, opts=>!opts.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(LoadoutOptions);
			else if (s.IsReading)
				LoadoutOptions.RevertToDefault();
		}
		public abstract void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		#endregion

		GameEngineType IGameEngineVariant.EngineType { get { return GameEngineType.None; } }
		GameEngineBaseVariant IGameEngineVariant.BaseVariant { get { return this; } }
	};
}