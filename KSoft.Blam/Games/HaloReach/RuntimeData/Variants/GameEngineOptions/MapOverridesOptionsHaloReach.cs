using System;

namespace KSoft.Blam.Games.HaloReach.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsMapOverridesHaloReach
		: Blam.RuntimeData.Variants.GameOptionsMapOverrides
	{
		public PlayerTraits Red { get; private set; }
		public PlayerTraits Blue { get; private set; }
		public PlayerTraits Yellow { get; private set; }
		public byte RedDuration, BlueDuration, YellowDuration;

		internal GameOptionsMapOverridesHaloReach(GameEngineBaseVariantHaloReach variant) : base(variant)
		{
			Red = new PlayerTraits();
			Blue = new PlayerTraits();
			Yellow = new PlayerTraits();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			SerializeFlags(s);
			s.StreamObject(BaseTraits);
			s.Stream(ref WeaponSet, 8, signExtend:true);
			s.Stream(ref VehicleSet, 8, signExtend:true);
			s.StreamObject(Red);
			s.StreamObject(Blue);
			s.StreamObject(Yellow);
			s.Stream(ref RedDuration, 7);		// 0xB8
			s.Stream(ref BlueDuration, 7);		// 0xB9
			s.Stream(ref YellowDuration, 7);	// 0xBA
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			using (s.EnterCursorBookmark("Powerups"))
			{
				Predicate<PlayerTraits> traits_are_changed = obj => !obj.IsUnchanged;

				using (var bm = s.EnterCursorBookmarkOpt("Red", Red, traits_are_changed)) if (bm.IsNotNull)
				{	s.StreamAttribute("duration", ref RedDuration);
					s.StreamObject(Red);
				}
				using (var bm = s.EnterCursorBookmarkOpt("Blue", Blue, traits_are_changed)) if (bm.IsNotNull)
				{	s.StreamAttribute("duration", ref BlueDuration);
					s.StreamObject(Blue);
				}
				using (var bm = s.EnterCursorBookmarkOpt("Yellow", Yellow, traits_are_changed)) if (bm.IsNotNull)
				{	s.StreamAttribute("duration", ref YellowDuration);
					s.StreamObject(Yellow);
				}
			}
		}
		#endregion
	};
}