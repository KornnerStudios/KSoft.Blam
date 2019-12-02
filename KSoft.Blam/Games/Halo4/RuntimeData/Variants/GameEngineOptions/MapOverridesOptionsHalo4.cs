using System;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsPowerupTraits
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public PlayerTraits Traits { get; private set; }
		public byte Duration;

		public bool IsUnchanged { get { return Duration == 0 && Traits.IsUnchanged; } }

		public GameOptionsPowerupTraits()
		{
			Traits = new PlayerTraits();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.StreamObject(Traits);
			s.Stream(ref Duration, 7);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("duration", ref Duration);
			s.StreamObject(Traits);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsPowerup
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsPowerupTraits BaseTraits { get; private set; }
		public GameOptionsPowerupTraits RuntimeTraits { get; private set; }

		public bool IsUsed { get { return BaseTraits.Duration > 0 || RuntimeTraits.Duration > 0; } }
		public bool IsUnchanged { get { return BaseTraits.IsUnchanged && RuntimeTraits.IsUnchanged; } }

		public GameOptionsPowerup()
		{
			BaseTraits = new GameOptionsPowerupTraits();
			RuntimeTraits = new GameOptionsPowerupTraits();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.StreamObject(BaseTraits);
			s.StreamObject(RuntimeTraits);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Base", BaseTraits, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				s.StreamObject(BaseTraits);
			using (var bm = s.EnterCursorBookmarkOpt("Runtime", RuntimeTraits, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				s.StreamObject(RuntimeTraits);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsMapOverridesHalo4
		: Blam.RuntimeData.Variants.GameOptionsMapOverrides
	{
		internal int EquipmentSet; // actually shorts at runtime
		// damage boost, speed boost, overshield
		// 4th = unused custom?
		public GameOptionsPowerup[] Powerups { get; private set; }

		protected override bool ObjectSetsAreNotDefault { get {
			return base.ObjectSetsAreNotDefault || EquipmentSet != TypeExtensionsBlam.kDefaultOption;
		} }

		public GameOptionsMapOverridesHalo4(GameEngineBaseVariantHalo4 variant) : base(variant)
		{
			EquipmentSet = TypeExtensionsBlam.kDefaultOption;
			Powerups = new GameOptionsPowerup[4];
			for (int x = 0; x < Powerups.Length; x++)
				Powerups[x] = new GameOptionsPowerup();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeFlags(s);
			s.StreamObject(BaseTraits);
			s.Stream(ref WeaponSet, 8, signExtend:true);
			s.Stream(ref VehicleSet, 8, signExtend:true);
			s.Stream(ref EquipmentSet, 8, signExtend:true);
			foreach (var pu in Powerups) s.StreamObject(pu);	// 0x4E0
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeObjectSets<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOptDefaultOption("equipment", ref EquipmentSet);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			using (s.EnterCursorBookmark("Powerups"))
			{
				GameOptionsPowerup pu;
				Predicate<GameOptionsPowerup> pu_is_not_default = obj => !obj.IsUnchanged;

				pu = Powerups[0];
				using (var bm = s.EnterCursorBookmarkOpt("DamageBoost", pu, pu_is_not_default)) if (bm.IsNotNull)
					s.StreamObject(pu);
				pu = Powerups[1];
				using (var bm = s.EnterCursorBookmarkOpt("SpeedBoost", pu, pu_is_not_default)) if (bm.IsNotNull)
					s.StreamObject(pu);
				pu = Powerups[2];
				using (var bm = s.EnterCursorBookmarkOpt("Overshield", pu, pu_is_not_default)) if (bm.IsNotNull)
					s.StreamObject(pu);
				pu = Powerups[3];
				using (var bm = s.EnterCursorBookmarkOpt("Custom", pu, pu_is_not_default)) if (bm.IsNotNull)
					s.StreamObject(pu);
			}
		}
		#endregion
	};
}