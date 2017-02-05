using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameOptionsSingleLoadoutFlagsBitStreamer = IO.EnumBitStreamer<GameOptionsSingleLoadoutFlags>;
	using GameOptionsLoadoutFlagsBitStreamer = IO.EnumBitStreamer<GameOptionsLoadoutFlags>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsSingleLoadoutFlags : byte
	{
		Enabled = 1<<0,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsLoadout
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsSingleLoadoutFlags Flags;
		public int NameIndex;
		public sbyte PrimaryWeapon, SecondaryWeapon, ArmorAbility;
		public byte InitialGrenadeCount;

		public bool IsUsed { get { return NameIndex.IsNotNone(); } }
		public bool IsHidden { get { return (Flags & GameOptionsSingleLoadoutFlags.Enabled) == 0; } }

		public virtual bool IsDefault { get {
			return NameIndex.IsNone() && Flags == 0 && InitialGrenadeCount == 0 &&
				PrimaryWeapon == TypeExtensionsBlam.kUnchanged && SecondaryWeapon == TypeExtensionsBlam.kUnchanged &&
				ArmorAbility == TypeExtensionsBlam.kUnchanged;
		} }

		public virtual void RevertToDefault()
		{
			Flags = 0;
			NameIndex = TypeExtensions.kNone;
			PrimaryWeapon = SecondaryWeapon = ArmorAbility = TypeExtensionsBlam.kUnchanged;
			InitialGrenadeCount = 0;
		}

		#region IBitStreamSerializable Members
		protected virtual void SerializeWeaponsAndEquipment(IO.BitStream s)
		{
			s.Stream(ref PrimaryWeapon);
			s.Stream(ref SecondaryWeapon);
			s.Stream(ref ArmorAbility);
		}
		public virtual void Serialize(IO.BitStream s)
		{
			s.Stream(ref Flags, 1, GameOptionsSingleLoadoutFlagsBitStreamer.Instance);
			s.StreamIndex(ref NameIndex, 7);
			SerializeWeaponsAndEquipment(s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected virtual void SerializeWeaponsAndEquipment<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOptUnchanged("primaryWeapon", ref PrimaryWeapon);
			s.StreamAttributeOptUnchanged("secondaryWeapon", ref SecondaryWeapon);
			s.StreamAttributeOptUnchanged("armorAbility", ref ArmorAbility);
		}
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags=> flags != 0, true);
			s.StreamAttributeOptNoneOption("nameIndex", ref NameIndex);
			SerializeWeaponsAndEquipment(s);
			s.StreamAttributeOpt("grenadeCount", ref InitialGrenadeCount, Predicates.IsNotZero);
		}
		#endregion
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsLoadoutPalette
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public abstract GameOptionsLoadout[] Loadouts { get; }

		public bool IsUsed { get {
			return Loadouts.TrueForAny(lo => lo.IsUsed);
		} }

		public bool IsDefault { get {
			return Array.TrueForAll(Loadouts, lo => lo.IsDefault);
		} }

		public virtual void RevertToDefault()
		{
			foreach (var lo in Loadouts)
				lo.RevertToDefault();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			foreach (var lo in Loadouts)
				lo.Serialize(s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected abstract void SerializeLoadouts<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, bool isUsed)
			where TDoc : class
			where TCursor : class;
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool is_used = s.IsWriting ? IsUsed : false;
			s.StreamAttribute("used", ref is_used);

			// FIXME: I noticed in H4's ricochet that the first two Palettes weren't default, but their names were NONE still, so this wouldn't write their non-default values out (but who cares?)
			if (is_used)
				SerializeLoadouts(s, is_used);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsLoadoutFlags : byte
	{
		MapLoadoutsEnabled = 1<<0, // ?
		Unknown1 = 1<<1,
		Unknown2 = 1<<2,				// Halo4
		PersonalLoadoutsEnabled = 1<<3,	// Halo4
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsLoadouts
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsLoadoutFlags Flags;
		public GameOptionsLoadoutPalette[] Palettes { get; protected set; }

		public bool IsDefault { get {
			return Flags == 0 && Array.TrueForAll(Palettes, p => p.IsDefault);
		} }

		public virtual void RevertToDefault()
		{
			Flags = 0;
			foreach (var p in Palettes)
				p.RevertToDefault();
		}

		#region IBitStreamSerializable Members
		protected void SerializeFlags(IO.BitStream s, int flagsBitLength)
		{
			s.Stream(ref Flags, flagsBitLength, GameOptionsLoadoutFlagsBitStreamer.Instance);
		}
		protected void SerializePalettes(IO.BitStream s)
		{
			foreach (var p in Palettes) p.Serialize(s);
		}
		public abstract void Serialize(IO.BitStream s);
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			Contract.Assert(Palettes.Length == 6, "Need to redo the streaming logic!");

			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);
			GameOptionsLoadoutPalette p;
			Predicate<GameOptionsLoadoutPalette> p_is_not_default = obj => !obj.IsDefault;

			p = Palettes[0];
			using (var bm = s.EnterCursorBookmarkOpt("Palette0", p, p_is_not_default)) p.Serialize(s);
			p = Palettes[1];
			using (var bm = s.EnterCursorBookmarkOpt("Palette1", p, p_is_not_default)) p.Serialize(s);
			p = Palettes[2];
			using (var bm = s.EnterCursorBookmarkOpt("Palette2", p, p_is_not_default)) p.Serialize(s);
			p = Palettes[3];
			using (var bm = s.EnterCursorBookmarkOpt("Palette3", p, p_is_not_default)) p.Serialize(s);
			p = Palettes[4];
			using (var bm = s.EnterCursorBookmarkOpt("Palette4", p, p_is_not_default)) p.Serialize(s);
			p = Palettes[5];
			using (var bm = s.EnterCursorBookmarkOpt("Palette5", p, p_is_not_default)) p.Serialize(s);
		}
		#endregion
	};
}