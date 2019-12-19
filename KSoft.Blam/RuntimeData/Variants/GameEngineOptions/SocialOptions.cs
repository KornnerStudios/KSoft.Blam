using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameOptionsSocialFlagsBitStreamer = IO.EnumBitStreamerWithOptions
		< GameOptionsSocialFlags
		, IO.EnumBitStreamerOptions.ShouldBitSwap>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsSocialFlags : byte // bits are individually streamed in the engine
	{
		FriendlyFireEnabled = 1<<0,
		BetrayalBootingEnabled = 1<<1,
		EnemyVoiceEnabled = 1<<2,
		OpenChannelVoiceEnabled = 1<<3,
		DeadPlayerVoiceEnabled = 1<<4,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsSocial
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		const GameOptionsSocialFlags kDefaultFlags = GameOptionsSocialFlags.FriendlyFireEnabled | GameOptionsSocialFlags.BetrayalBootingEnabled |
				GameOptionsSocialFlags.EnemyVoiceEnabled;
		public bool ObserversAllowed; // always false; deprecated
		public byte TeamChanging;
		public GameOptionsSocialFlags Flags;

		public bool IsDefault { get {
			return ObserversAllowed == false && TeamChanging == (int)TeamChangingType.Disabled && Flags == kDefaultFlags;
		} }

		public GameOptionsSocial()
		{
			RevertToDefault();
		}

		public /*virtual*/ void RevertToDefault()
		{
			ObserversAllowed = false;
			TeamChanging = (byte)TeamChangingType.Enabled;
			Flags = kDefaultFlags;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref ObserversAllowed);					Contract.Assert(ObserversAllowed==false);
			s.Stream(ref TeamChanging, 2);
			s.Stream(ref Flags, 5, GameOptionsSocialFlagsBitStreamer.Instance);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var teamChanging = s.IsReading ? TeamChangingType.Enabled : (TeamChangingType)TeamChanging;

			//s.StreamAttributeOpt("observersAllowed", ref ObserversAllowed, Predicates.IsTrue);
			s.StreamAttributeEnumOpt("teamChanging", ref teamChanging, v => v != TeamChangingType.Enabled);
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != kDefaultFlags, true);

			if (s.IsReading)
			{
				TeamChanging = (byte)teamChanging;
			}
		}
		#endregion
	};
}