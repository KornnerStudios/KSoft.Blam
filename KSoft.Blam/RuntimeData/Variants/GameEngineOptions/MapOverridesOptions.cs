using System;

namespace KSoft.Blam.RuntimeData.Variants
{
	using GameOptionsMapOverridesFlagsBitStreamer = IO.EnumBitStreamer<GameOptionsMapOverridesFlags>;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsMapOverridesFlags : byte
	{
		GrenadesOnMap = 1<<0,
		ShortcutsOnMap = 1<<1,
		EquipmentOnMap = 1<<2,
		PowerupsOnMap = 1<<3,
		TurretsOnMap = 1<<4,
		IndestructibleVehiclesEnabled = 1<<5,
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class GameOptionsMapOverrides
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsMapOverridesFlags Flags;
		public PlayerTraitsBase BaseTraits { get; private set; }

		public int WeaponSet, VehicleSet; // actually shorts at runtime

		protected virtual bool ObjectSetsAreNotDefault { get {
			return WeaponSet != TypeExtensionsBlam.kDefaultOption || VehicleSet != TypeExtensionsBlam.kDefaultOption;
		} }

		protected GameOptionsMapOverrides(GameEngineBaseVariant variant)
		{
			BaseTraits = variant.NewPlayerTraits();
			WeaponSet = VehicleSet = TypeExtensionsBlam.kDefaultOption;
		}

		#region IBitStreamSerializable Members
		protected void SerializeFlags(IO.BitStream s)
		{
			s.Stream(ref Flags, 6, GameOptionsMapOverridesFlagsBitStreamer.Instance);
		}
		public abstract void Serialize(IO.BitStream s);
		#endregion
		#region ITagElementStringNameStreamable Members
		protected virtual void SerializeObjectSets<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOptDefaultOption("weapons", ref WeaponSet);
			s.StreamAttributeOptDefaultOption("vehicles", ref VehicleSet);
		}
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);
			using (var bm = s.EnterCursorBookmarkOpt("ObjectSets", this, obj=>obj.ObjectSetsAreNotDefault)) if (bm.IsNotNull)
				SerializeObjectSets(s);
			using (var bm = s.EnterCursorBookmarkOpt("BaseTraits", BaseTraits, t=>!t.IsUnchanged)) if(bm.IsNotNull)
				s.StreamObject(BaseTraits);
		}
		#endregion
	};
}