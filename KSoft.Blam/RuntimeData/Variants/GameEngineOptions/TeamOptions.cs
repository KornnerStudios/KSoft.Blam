using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.RuntimeData.Variants
{
	using LocaleStringTableInfo = Localization.StringTables.LocaleStringTableInfo;
	using LocaleStringTable = Localization.StringTables.LocaleStringTable;

	using GameOptionsSingleTeamFlagsBitStreamer = IO.EnumBitStreamer<GameOptionsSingleTeamFlags>;
	using GameOptionsSingleTeamModelOverrideBitStreamer = IO.EnumBitStreamer<GameOptionsSingleTeamModelOverride>;
	using GameOptionsTeamOptionsModelOverrideBitStreamer = IO.EnumBitStreamer<GameOptionsTeamOptionsModelOverride>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsSingleTeamFlags : byte
	{
		Enabled = 1<<0,
		PrimaryColorOverrideEnabled = 1<<1,
		SecondaryColorOverrideEnabled = 1<<2,
		TextColorOverrideEnabled = 1<<3,
		InterfaceColorOverrideEnabled = 1<<4,	// Halo4
		EmblemOverrideEnabled = 1<<5,			// Halo4
	};
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum GameOptionsSingleTeamModelOverride : byte
	{
		Spartan,
		Elite,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kMax,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsSingleTeamOptions
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsSingleTeamFlags Flags;
		public LocaleStringTable NameString { get; private set; }
		public int InitialDesignator = TypeExtensions.kNone;
		public GameOptionsSingleTeamModelOverride ModelOverride;
		public uint PrimaryColorOverride, SecondaryColorOverride, TextColorOverride;
		public int FireteamCount;

		public bool IsUsed { get { return InitialDesignator.IsNotNone(); } }

		public virtual bool OverridesAreDefault { get {
			return PrimaryColorOverride == uint.MaxValue && SecondaryColorOverride == uint.MaxValue &&
				TextColorOverride == uint.MaxValue &&
				ModelOverride == GameOptionsSingleTeamModelOverride.Spartan;
		} }

		public virtual bool IsDefault { get {
			return Flags == 0 && NameString.Count == 0 && InitialDesignator.IsNone() && 
				OverridesAreDefault && FireteamCount == 1;
		} }

		protected GameOptionsSingleTeamOptions(LocaleStringTableInfo nameInfo)
		{
			NameString = new LocaleStringTable(nameInfo);
		}

		public virtual void RevertToDefault()
		{
			Flags = 0;
			NameString.Clear();
			InitialDesignator = TypeExtensions.kNone;
			PrimaryColorOverride = SecondaryColorOverride = TextColorOverride = uint.MaxValue;
			ModelOverride = GameOptionsSingleTeamModelOverride.Spartan;
			FireteamCount = 1;
		}

		#region IBitStreamSerializable Members
		protected void SerializeFlags(IO.BitStream s, int flagsBitLength)
		{
			s.Stream(ref Flags, flagsBitLength, GameOptionsSingleTeamFlagsBitStreamer.Instance);
		}
		protected void SerializeModelOverride(IO.BitStream s)
		{
			s.Stream(ref ModelOverride, 1, GameOptionsSingleTeamModelOverrideBitStreamer.Instance);
		}
		public abstract void Serialize(IO.BitStream s);
		#endregion
		#region ITagElementStringNameStreamable Members
		protected virtual void SerializeOverrides<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("primaryColor", ref PrimaryColorOverride, v=>v!=uint.MaxValue, NumeralBase.Hex);
			s.StreamAttributeOpt("secondaryColor", ref SecondaryColorOverride, v=>v!=uint.MaxValue, NumeralBase.Hex);
			s.StreamAttributeOpt("textColor", ref TextColorOverride, v=>v!=uint.MaxValue, NumeralBase.Hex);

			s.StreamAttributeEnumOpt("model", ref ModelOverride, e => e != GameOptionsSingleTeamModelOverride.Spartan);
		}
		protected virtual void SerializeImpl<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);
			s.StreamAttributeOpt("fireteamCount", ref FireteamCount, v=>v!=1);

			using (var bm = s.EnterCursorBookmarkOpt("Overrides", this, obj=>!obj.OverridesAreDefault)) if(bm.IsNotNull)
				SerializeOverrides(s);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("designator", ref InitialDesignator);
			if (InitialDesignator.IsNone())
			{
				RevertToDefault();
				return;
			}

			SerializeImpl(s);

			using (var bm = s.EnterCursorBookmarkOpt("Name", NameString, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamObject(NameString);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum GameOptionsTeamOptionsModelOverride : byte
	{
		Default,
		Spartan,
		Elite,
		TeamDefault,
		Survival, // anyone on NONE team (ie, not on a team) use elite models

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kMax,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsTeamOptions
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsTeamOptionsModelOverride ModelOverride;
		public byte DesignatorSwitchType = 2;
		public abstract GameOptionsSingleTeamOptions[] Teams { get; }

		public virtual bool IsDefault { get {
			return ModelOverride == GameOptionsTeamOptionsModelOverride.Default && DesignatorSwitchType == 2 &&
				Array.TrueForAll(Teams, t => t == null || t.IsDefault);
		} }

		public virtual void RevertToDefault()
		{
			ModelOverride = GameOptionsTeamOptionsModelOverride.Default;
			DesignatorSwitchType = 2;
			foreach (var team in Teams)
				team.RevertToDefault();
		}

		#region IBitStreamSerializable Members
		protected void SerializeModelOverride(IO.BitStream s)
		{
			s.Stream(ref ModelOverride, 3, GameOptionsTeamOptionsModelOverrideBitStreamer.Instance);
		}
		public abstract void Serialize(IO.BitStream s);
		#endregion
		#region ITagElementStringNameStreamable Members
		public abstract void SerializeTeams<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("designatorSwitchType", ref DesignatorSwitchType, v=>v!=2);
			s.StreamAttributeEnumOpt("model", ref ModelOverride, e => e != GameOptionsTeamOptionsModelOverride.Default);
			SerializeTeams(s);
		}
		#endregion
	};
}