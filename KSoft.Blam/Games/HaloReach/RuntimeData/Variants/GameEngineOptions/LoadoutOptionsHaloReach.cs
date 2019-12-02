
namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsLoadoutHaloReach
		: Blam.RuntimeData.Variants.GameOptionsLoadout
	{
		public GameOptionsLoadoutHaloReach()
		{
			RevertToDefault();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			base.Serialize(s);

			s.Stream(ref InitialGrenadeCount, 4);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsLoadoutPaletteHaloReach
		: Blam.RuntimeData.Variants.GameOptionsLoadoutPalette
	{
		readonly GameOptionsLoadoutHaloReach[] mLoadouts;
		public override Blam.RuntimeData.Variants.GameOptionsLoadout[] Loadouts { get { return mLoadouts; } }

		public GameOptionsLoadoutPaletteHaloReach()
		{
			mLoadouts = new GameOptionsLoadoutHaloReach[5];

			for (int x = 0; x < Loadouts.Length; x++)
				mLoadouts[x] = new GameOptionsLoadoutHaloReach();
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
	public sealed class GameOptionsLoadoutsHaloReach
		: Blam.RuntimeData.Variants.GameOptionsLoadouts
	{
		public GameOptionsLoadoutsHaloReach()
		{
			Palettes = new GameOptionsLoadoutPaletteHaloReach[6];

			for (int x = 0; x < Palettes.Length; x++)
				Palettes[x] = new GameOptionsLoadoutPaletteHaloReach();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeFlags(s, 2);
			SerializePalettes(s);
		}
		#endregion
	};
}