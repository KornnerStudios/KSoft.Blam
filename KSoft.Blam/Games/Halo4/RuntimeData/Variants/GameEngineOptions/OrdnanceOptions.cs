using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	using GameOptionsOrdnanceOptionsFlagsBitStreamer = IO.EnumBitStreamerWithOptions
		< GameOptionsOrdnanceOptionsFlags
		, IO.EnumBitStreamerOptions.ShouldBitSwap
		>;

	[System.Reflection.Obfuscation(Exclude=false)]
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct GameOptionsOrdnancePossibility
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		const float kDefaultWeight = 1.000003f; // engine=1

		public string Name;
		public float Weight;

		public bool IsDefault =>
			Name.IsNullOrEmpty() && Weight == kDefaultWeight;

		public void RevertToDefault()
		{
			Name = "";
			Weight = kDefaultWeight;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Name, Memory.Strings.StringStorage.CStringAscii, TypeExtensionsBlam.kTagStringLength);
			s.Stream(ref Weight, 0.0f, 10000.0f, 30, false, true);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("weight", ref Weight, Predicates.IsNotZero);
			if (!s.StreamAttributeOpt("name", ref Name, Predicates.IsNotNullOrEmpty))
				Name = "";
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class GameOptionsOrdnancePersonalOrdnancePossibilities
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public GameOptionsOrdnancePossibility[] Possibilities;

		public bool IsDefault { get {
			foreach (var poss in Possibilities)
				if (!poss.IsDefault)
					return false;

			return true;
		} }

		public GameOptionsOrdnancePersonalOrdnancePossibilities()
		{
			Possibilities = new GameOptionsOrdnancePossibility[8];
			RevertToDefault();
		}

		public void RevertToDefault()
		{
			for (int x = 0; x < Possibilities.Length; x++)
				Possibilities[x].RevertToDefault();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			for (int x = 0; x < Possibilities.Length; x++)
				s.StreamValue(ref Possibilities[x]);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamableFixedArray("Possibility", Possibilities);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameOptionsOrdnanceOptionsFlags : byte // engine uses individual bools
	{
		InitialOrdnanceEnabled = 1<<0,
		RandomOrdnanceEnabled = 1<<1,
		ObjectiveOrdnanceEnabled = 1<<2,
		PersonalOrdnanceEnabled = 1<<3,
		OrdnanceEnabled = 1<<4, // 0x5 at runtime
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	public class GameOptionsOrdnanceOptions
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		const GameOptionsOrdnanceOptionsFlags kDefaultFlags =
			GameOptionsOrdnanceOptionsFlags.InitialOrdnanceEnabled |
			GameOptionsOrdnanceOptionsFlags.RandomOrdnanceEnabled |
			GameOptionsOrdnanceOptionsFlags.ObjectiveOrdnanceEnabled |
			GameOptionsOrdnanceOptionsFlags.PersonalOrdnanceEnabled |
			GameOptionsOrdnanceOptionsFlags.OrdnanceEnabled
			;
		const float kDefaultPointRequirement = 70.00001f; // engine=70
		const float kDefaultPointIncreaseMultiplier = 0.300002277f; // 0.3000023f, engine=0.30000001f

		public GameOptionsOrdnanceOptionsFlags Flags;
		public int unk6; // sbyte at runtime
		public int ResupplyTimeMin, ResupplyTimeMax; // seconds; shorts at runtime
		public int unk12; // short at runtime
		public string InitialDropSet;
		public int unk36, InitialDropDelay; // short at runtime; inital drop set related?
		public string RandomDropSet, PersonalDropSet, OrdnanceSubstitutions;
		public bool CustomizePersonalOrdnance; // 0x4 at runtime
		// right, left, down (middle), up? (not exposed; unused?)
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public GameOptionsOrdnancePersonalOrdnancePossibilities[] PersonalOrdnance { get; private set; }
		float PointRequirement, PointIncreaseMultiplier;

		#region IsDefault
		bool InfinityResupplyTime_IsUnchanged { get {
			return ResupplyTimeMin == TypeExtensionsBlam.kDefaultOption && ResupplyTimeMax == TypeExtensionsBlam.kDefaultOption;
		} }
		bool Points_AreUnchanged { get {
			return PointRequirement == kDefaultPointRequirement && PointIncreaseMultiplier == kDefaultPointIncreaseMultiplier;
		} }
		bool DropSetNames_AreDefault { get {
			return InitialDropSet_NeedsDefaultHack && RandomDropSet_NeedsDefaultHack && PersonalDropSet_NeedsDefaultHack &&
				string.IsNullOrEmpty(OrdnanceSubstitutions);
		} }

		const string kDefaultString = "\x3F";
		static readonly Predicate<string> IsNotDefaultString = s => s != kDefaultString;
			//s.Length != 1 || s[0] != 0x3F;

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public bool InitialDropSet_NeedsDefaultHack { get { return !IsNotDefaultString(InitialDropSet); } }
		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public bool RandomDropSet_NeedsDefaultHack { get { return !IsNotDefaultString(RandomDropSet); } }
		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public bool PersonalDropSet_NeedsDefaultHack { get { return !IsNotDefaultString(PersonalDropSet); } }
		public bool OrdnanceSubstitutionsNeedsDefaultHack { get { return !IsNotDefaultString(OrdnanceSubstitutions); } }

		public bool IsDefault { get {
			return Flags == kDefaultFlags && unk6 == TypeExtensionsBlam.kDefaultOption && InfinityResupplyTime_IsUnchanged &&
				unk12 == 0 && unk36 == 0 && InitialDropDelay == 0 && DropSetNames_AreDefault && CustomizePersonalOrdnance == false &&
				Points_AreUnchanged && Array.TrueForAll(PersonalOrdnance, o=>o.IsDefault);
		} }
		#endregion

		public GameOptionsOrdnanceOptions()
		{
			PersonalOrdnance = new GameOptionsOrdnancePersonalOrdnancePossibilities[4];

			for (int x = 0; x < PersonalOrdnance.Length; x++)
				PersonalOrdnance[x] = new GameOptionsOrdnancePersonalOrdnancePossibilities();

			RevertToDefault();
		}

		public void RevertToDefault()
		{
			Flags = kDefaultFlags;

			unk6 = TypeExtensionsBlam.kDefaultOption;
			ResupplyTimeMin = ResupplyTimeMax = 0;

			unk12 = unk36 = InitialDropDelay = 0;
			InitialDropSet = RandomDropSet = PersonalDropSet = kDefaultString;
			OrdnanceSubstitutions = "";
			CustomizePersonalOrdnance = false;

			for (int x = 0; x < PersonalOrdnance.Length; x++)
				PersonalOrdnance[x].RevertToDefault();

			PointRequirement = kDefaultPointRequirement;
			PointIncreaseMultiplier = kDefaultPointIncreaseMultiplier;
		}

		#region IBitStreamSerializable Members
		void WriteDefaultHack(IO.BitStream s)
		{
			s.Write((sbyte)TypeExtensionsBlam.kDefaultOption);
			s.Write(byte.MinValue);
		}
		public void Serialize(IO.BitStream s)
		{
			bool writing = s.IsWriting;

			s.Stream(ref Flags, 5, GameOptionsOrdnanceOptionsFlagsBitStreamer.Instance);
			s.Stream(ref unk6, Bits.kByteBitCount, signExtend:true);
			s.Stream(ref ResupplyTimeMin, Bits.kInt16BitCount, signExtend:true);
			s.Stream(ref ResupplyTimeMax, Bits.kInt16BitCount, signExtend:true);
			s.Stream(ref unk12, Bits.kInt16BitCount, signExtend:true);
			if (writing && InitialDropSet_NeedsDefaultHack)			WriteDefaultHack(s);
			else
				s.Stream(ref InitialDropSet, Memory.Strings.StringStorage.CStringAscii, TypeExtensionsBlam.kTagStringLength);
			s.Stream(ref unk36, Bits.kInt16BitCount, signExtend:true);
			s.Stream(ref InitialDropDelay, Bits.kInt16BitCount, signExtend:true);
			if (writing && RandomDropSet_NeedsDefaultHack)			WriteDefaultHack(s);
			else
				s.Stream(ref RandomDropSet, Memory.Strings.StringStorage.CStringAscii, TypeExtensionsBlam.kTagStringLength);
			if (writing && PersonalDropSet_NeedsDefaultHack)		WriteDefaultHack(s);
			else
				s.Stream(ref PersonalDropSet, Memory.Strings.StringStorage.CStringAscii, TypeExtensionsBlam.kTagStringLength);
			if (writing && OrdnanceSubstitutionsNeedsDefaultHack)	WriteDefaultHack(s);
			else
				s.Stream(ref OrdnanceSubstitutions, Memory.Strings.StringStorage.CStringAscii, TypeExtensionsBlam.kTagStringLength);
			s.Stream(ref CustomizePersonalOrdnance);
			foreach (var po in PersonalOrdnance) s.StreamObject(po);
			s.Stream(ref PointRequirement, 0.0f, 10000.0f, 30, false, true);
			s.Stream(ref PointIncreaseMultiplier, 0.0f, 10000.0f, 30, false, true);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);

			s.StreamAttributeOptDefaultOption("unk6", ref unk6);
			s.StreamAttributeOptDefaultOption("unk12", ref unk12);

			s.StreamAttributeOptDefaultOption("unk36", ref unk36);
			s.StreamAttributeOptDefaultOption("initialDropDelay", ref InitialDropDelay);

			s.StreamAttributeOpt("customizePersonalOrdnance", ref CustomizePersonalOrdnance, Predicates.IsTrue);

			using (var bm = s.EnterCursorBookmarkOpt("DropSets", this, obj=>!obj.DropSetNames_AreDefault)) if (bm.IsNotNull)
			{
				if (!s.StreamAttributeOpt("initial", ref InitialDropSet, IsNotDefaultString))
					InitialDropSet = kDefaultString;
				if (!s.StreamAttributeOpt("random", ref RandomDropSet, IsNotDefaultString))
					RandomDropSet = kDefaultString;
				if (!s.StreamAttributeOpt("personal", ref PersonalDropSet, IsNotDefaultString))
					PersonalDropSet = kDefaultString;
				if (!s.StreamAttributeOpt("ordnanceSubstitutions", ref OrdnanceSubstitutions, Predicates.IsNotNullOrEmpty))
					OrdnanceSubstitutions = "";
			}
			using (var bm = s.EnterCursorBookmarkOpt("InfinityResupplyTime", this, obj=>!obj.InfinityResupplyTime_IsUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttributeOptDefaultOption("min", ref ResupplyTimeMin);
				s.StreamAttributeOptDefaultOption("max", ref ResupplyTimeMax);
			}
			using (var bm = s.EnterCursorBookmarkOpt("Points", this, obj=>!obj.Points_AreUnchanged)) if (bm.IsNotNull)
			{
				s.StreamAttribute("requirement", ref PointRequirement);
				s.StreamAttribute("increaseMultiplier", ref PointIncreaseMultiplier);
			}

			#region PersonalOrdnance
			GameOptionsOrdnancePersonalOrdnancePossibilities possibilities;
			Predicate<GameOptionsOrdnancePersonalOrdnancePossibilities> possibilities_are_changed = p => !p.IsDefault;

			possibilities = PersonalOrdnance[0];
			using (var bm = s.EnterCursorBookmarkOpt("Right", possibilities, possibilities_are_changed)) if (bm.IsNotNull)
				s.StreamObject(possibilities);
			possibilities = PersonalOrdnance[1];
			using (var bm = s.EnterCursorBookmarkOpt("Left", possibilities, possibilities_are_changed)) if (bm.IsNotNull)
				s.StreamObject(possibilities);
			possibilities = PersonalOrdnance[2];
			using (var bm = s.EnterCursorBookmarkOpt("Down", possibilities, possibilities_are_changed)) if (bm.IsNotNull)
				s.StreamObject(possibilities);
			possibilities = PersonalOrdnance[3];
			using (var bm = s.EnterCursorBookmarkOpt("Unused", possibilities, possibilities_are_changed)) if (bm.IsNotNull)
				s.StreamObject(possibilities);
			#endregion
		}
		#endregion
	};
}
