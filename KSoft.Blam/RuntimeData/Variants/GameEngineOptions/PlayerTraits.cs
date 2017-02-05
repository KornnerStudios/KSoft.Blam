using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.RuntimeData.Variants
{
	partial class GameEngineBaseVariant
	{
		internal abstract PlayerTraitsBase NewPlayerTraits();
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class PlayerTraitsDataBase
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		protected virtual bool ModifiersAreUnchanged { get { return true; } }
		public abstract bool IsUnchanged { get; }

		public abstract void Serialize(IO.BitStream s);

		#region ITagElementStringNameStreamable Members
		protected virtual void SerializeModifiers<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
		}
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Modifiers", this, obj=>!obj.ModifiersAreUnchanged)) if (bm.IsNotNull)
				SerializeModifiers(s);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class PlayerTraitsDamageBase : PlayerTraitsDataBase
	{
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class PlayerTraitsWeaponsBase : PlayerTraitsDataBase
	{
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class PlayerTraitsMovementBase : PlayerTraitsDataBase
	{
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class PlayerTraitsAppearanceBase : PlayerTraitsDataBase
	{
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class PlayerTraitsSensorsBase : PlayerTraitsDataBase
	{
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class PlayerTraitsBase
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public PlayerTraitsDamageBase Damage { get; protected set; }
		public PlayerTraitsWeaponsBase Weapons { get; protected set; }
		public PlayerTraitsMovementBase Movement { get; protected set; }
		public PlayerTraitsAppearanceBase Appearance { get; protected set; }
		public PlayerTraitsSensorsBase Sensors { get; protected set; }

		public virtual bool IsUnchanged { get {
			return Damage.IsUnchanged && Weapons.IsUnchanged && Movement.IsUnchanged && Appearance.IsUnchanged &&
				Sensors.IsUnchanged;
		} }

		#region IBitStreamSerializable Members
		public virtual void Serialize(IO.BitStream s)
		{
			Damage.Serialize(s);
			Weapons.Serialize(s);
			Movement.Serialize(s);
			Appearance.Serialize(s);
			Sensors.Serialize(s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Damage", Damage, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				s.StreamObject(Damage);

			using (var bm = s.EnterCursorBookmarkOpt("Weapons", Weapons, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				s.StreamObject(Weapons);

			using (var bm = s.EnterCursorBookmarkOpt("Movement", Movement, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				s.StreamObject(Movement);

			using (var bm = s.EnterCursorBookmarkOpt("Appearance", Appearance, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				s.StreamObject(Appearance);

			using (var bm = s.EnterCursorBookmarkOpt("Sensors", Sensors, obj=>!obj.IsUnchanged)) if(bm.IsNotNull)
				s.StreamObject(Sensors);
		}
		#endregion
	};
}