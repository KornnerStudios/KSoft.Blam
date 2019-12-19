using System;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	using GameOptionsLoadoutFlagsBitStreamer = IO.EnumBitStreamer<GameOptionsLoadoutFlagsHalo4>;

	[Flags]
	public enum GameOptionsLoadoutFlagsHalo4
	{
		CustomLoadouts = 1<<0,
		SpartanLoadouts = 1<<1,
		EliteLoadouts = 1<<2,
		MapLoadouts = 1<<3,
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsLoadoutHalo4
		: Blam.RuntimeData.Variants.GameOptionsLoadout
	{
		public sbyte TacticalPackage, SupportUpgrade;

		public byte PrimaryWeaponSkin, SecondaryWeaponSkin;

		bool WeaponSkinsAreUnchanged { get { return PrimaryWeaponSkin == 0 && SecondaryWeaponSkin == 0; } }

		public override bool IsDefault { get {
			return base.IsDefault &&
				TacticalPackage == TypeExtensionsBlam.kUnchanged && SupportUpgrade == TypeExtensionsBlam.kUnchanged &&
				WeaponSkinsAreUnchanged;
		} }

		public GameOptionsLoadoutHalo4()
		{
			RevertToDefault();
		}

		public override void RevertToDefault()
		{
			base.RevertToDefault();

			TacticalPackage = SupportUpgrade = TypeExtensionsBlam.kUnchanged;
			PrimaryWeaponSkin = SecondaryWeaponSkin = 0;
		}

		#region IBitStreamSerializable Members
		protected override void SerializeWeaponsAndEquipment(IO.BitStream s)
		{
			base.SerializeWeaponsAndEquipment(s);

			s.Stream(ref TacticalPackage);
			s.Stream(ref SupportUpgrade);
		}
		public override void Serialize(IO.BitStream s)
		{
			base.Serialize(s);

			s.Stream(ref InitialGrenadeCount, 5);
			s.Stream(ref PrimaryWeaponSkin, 3);
			s.Stream(ref SecondaryWeaponSkin, 3);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeWeaponsAndEquipment<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.SerializeWeaponsAndEquipment(s);

			s.StreamAttributeOptUnchanged("tacticalPackage", ref TacticalPackage);
			s.StreamAttributeOptUnchanged("supportUpgrade", ref SupportUpgrade);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			using (var bm = s.EnterCursorBookmarkOpt("WeaponSkins", this, obj=>!obj.WeaponSkinsAreUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOpt("primary", ref PrimaryWeaponSkin, Predicates.IsNotZero);
				s.StreamAttributeOpt("secondary", ref SecondaryWeaponSkin, Predicates.IsNotZero);
			}
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsLoadoutPaletteHalo4
		: Blam.RuntimeData.Variants.GameOptionsLoadoutPalette
	{
		readonly GameOptionsLoadoutHalo4[] mLoadouts;
		public override Blam.RuntimeData.Variants.GameOptionsLoadout[] Loadouts { get { return mLoadouts; } }

		public GameOptionsLoadoutPaletteHalo4()
		{
			mLoadouts = new GameOptionsLoadoutHalo4[5];

			for (int x = 0; x < Loadouts.Length; x++)
				Loadouts[x] = new GameOptionsLoadoutHalo4();
		}

		#region ITagElementStringNameStreamable Members
		protected override void SerializeLoadouts<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, bool isUsed)
		{
			int streamed_count = !isUsed ? 0 : s.StreamableFixedArray("entry", mLoadouts);

			if (s.IsReading)
				for (; streamed_count < Loadouts.Length; streamed_count++)
					Loadouts[streamed_count].RevertToDefault();
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsLoadoutsHalo4
		: Blam.RuntimeData.Variants.GameOptionsLoadouts
	{
		const GameOptionsLoadoutFlagsHalo4 kDefaultFlags =
			GameOptionsLoadoutFlagsHalo4.CustomLoadouts |
			GameOptionsLoadoutFlagsHalo4.MapLoadouts;

		public GameOptionsLoadoutFlagsHalo4 Flags;

		public GameOptionsLoadoutsHalo4()
		{
			Palettes = new GameOptionsLoadoutPaletteHalo4[6];

			for (int x = 0; x < Palettes.Length; x++)
				Palettes[x] = new GameOptionsLoadoutPaletteHalo4();
		}

		public override bool IsDefault { get {
			return Flags == kDefaultFlags && base.IsDefault;
		} }

		public override void RevertToDefault()
		{
			base.RevertToDefault();

			Flags = kDefaultFlags;
		}

		#region IBitStreamSerializable Members
		protected override void SerializeLoadoutFlags(IO.BitStream s)
		{
			s.Stream(ref Flags, 4, GameOptionsLoadoutFlagsBitStreamer.Instance);
		}
		#endregion

		protected override void SerializeLoadoutFlags<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != kDefaultFlags, true);
		}
	};
}