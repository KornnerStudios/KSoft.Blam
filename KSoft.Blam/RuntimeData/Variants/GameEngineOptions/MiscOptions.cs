using System;

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameOptionsMiscFlagsBitStreamer = IO.EnumBitStreamerWithOptions
		< GameOptionsMiscFlags
		, IO.EnumBitStreamerOptions.ShouldBitSwap
		>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsMiscFlags : byte // bits are individually streamed in the engine
	{
		TeamsEnabled = 1<<0,
		ResetPlayersOnNewRound = 1<<1,
		ResetMapOnNewRound = 1<<2,
		PerfectionEnabled = 1<<3,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsMisc
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsMiscFlags Flags;
		public byte RoundTimeLimit, // minutes
			RoundLimit,
			EarlyVictoryWinCount;
		public int SuddenDeathTimeLimit; // seconds

		protected GameOptionsMisc()
		{
			SuddenDeathTimeLimit = 90;
		}

		#region IBitStreamSerializable Members
		protected void SerializeFlags(IO.BitStream s)
		{
			s.Stream(ref Flags, 4, GameOptionsMiscFlagsBitStreamer.Instance);
		}
		public abstract void Serialize(IO.BitStream s);
		#endregion
		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, f => f != 0, true);
			using (s.EnterCursorBookmark("Round"))
			{
				s.StreamAttribute("timeLimit", ref RoundTimeLimit);
				s.StreamAttribute("limit", ref RoundLimit);
				s.StreamAttribute("earlyVictoryWinCount", ref EarlyVictoryWinCount);
				s.StreamAttribute("suddenDeathTimeLimit", ref SuddenDeathTimeLimit);
			}
		}
		#endregion
	};
}