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
		VariantBuiltIn = 1<<0,
		UserCreatedVariant = 1<<1, // Halo4
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class GameEngineBaseVariant
		: IGameEngineVariant
	{
		public GameEngineBaseVariantFlags Flags;

		public Engine.EngineBuildHandle BuildHandle { get; private set; }

		public ContentHeader Header { get; private set; }

		public GameOptionsMisc OptionsMisc { get; protected set; } = null!;
		public GameOptionsRepawning OptionsRespawning { get; protected set; } = null!;

		public GameOptionsSocial OptionsSocial { get; protected set; } = new();

		public GameOptionsMapOverrides OptionsMapOverrides { get; protected set; } = null!;

		public GameOptionsTeamOptions TeamOptions { get; protected set; } = null!;
		public GameOptionsLoadouts LoadoutOptions { get; protected set; } = null!;

		protected GameEngineBaseVariant(GameEngineVariant variantManager)
		{
			BuildHandle = variantManager.GameBuild;
			Header = ContentHeader.Create(BuildHandle);
		}

		internal static GameEngineBaseVariant Create(GameEngineVariant variantManager)
		{
			ArgumentNullException.ThrowIfNull(variantManager);

			var game_build = variantManager.GameBuild;

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
			{
				return new Games.HaloReach.RuntimeData.Variants.GameEngineBaseVariantHaloReach(variantManager);
			}

			if (game_build.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
			{
				return new Games.Halo4.RuntimeData.Variants.GameEngineBaseVariantHalo4(variantManager);
			}

			throw new KSoft.Debug.UnreachableException(game_build.ToDisplayString());
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
			{
				s.StreamObject(Header);
			}
		}
		protected void SerializeMiscOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Misc"))
			{
				s.StreamObject(OptionsMisc);
			}
		}
		protected void SerializRespawnOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Respawning"))
			{
				s.StreamObject(OptionsRespawning);
			}
		}
		protected void SerializeSocialOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Social", OptionsSocial, opts=>!opts.IsDefault))
			{
				if (bm.IsNotNull)
				{
					s.StreamObject(OptionsSocial);
				}
				else if (s.IsReading)
				{
					OptionsSocial.RevertToDefault();
				}
			}
		}
		protected void SerializMapOverrides<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("MapOverrides"))
			{
				s.StreamObject(OptionsMapOverrides);
			}
		}
		protected void SerializeTeams<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Teams", TeamOptions, opts=>!opts.IsDefault))
			{
				if (bm.IsNotNull)
				{
					s.StreamObject(TeamOptions);
				}
				else if (s.IsReading)
				{
					TeamOptions.RevertToDefault();
				}
			}
		}
		protected void SerializeLoadoutOptions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Loadouts", LoadoutOptions, opts=>!opts.IsDefault))
			{
				if (bm.IsNotNull)
				{
					s.StreamObject(LoadoutOptions);
				}
				else if (s.IsReading)
				{
					LoadoutOptions.RevertToDefault();
				}
			}
		}
		public abstract void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		#endregion

		GameEngineType IGameEngineVariant.EngineType => GameEngineType.None;
		GameEngineBaseVariant IGameEngineVariant.BaseVariant => this;
	};
}
