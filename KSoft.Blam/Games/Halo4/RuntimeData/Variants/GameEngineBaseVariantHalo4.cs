using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	using GameOptionsOrdnanceOptionsFlagsBitStreamer = IO.EnumBitStreamerWithOptions
		< GameOptionsOrdnanceOptionsFlags
		, IO.EnumBitStreamerOptions.ShouldBitSwap
		>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameOptionsPrototype
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public byte Mode,			// max: 2
			// energy percent
			PrometheanEnergyKill,	// max: 7
			PrometheanEnergyTime,	// max: 7
			PrometheanEnergyMedal,	// max: 9
			PrometheanDuration;		// beta max: 9
		bool unk2C1;

		public bool IsEmpty { get {
			return Mode == 0 && PrometheanEnergyKill == 0 && PrometheanEnergyTime == 0 &&
				PrometheanEnergyMedal == 0 && PrometheanDuration == 0 && unk2C1 == false;
		} }

		#region IsDefault
		bool IsDefaultPrometheanMode0 { get {
			return PrometheanEnergyKill == 0 && PrometheanEnergyTime == 0 &&
				PrometheanEnergyMedal == 0 && PrometheanDuration == 0;
		} }
		bool IsDefaultPrometheanMode1 { get {
			return PrometheanEnergyKill == 3 && PrometheanEnergyTime == 3 &&
				PrometheanEnergyMedal == 3 && PrometheanDuration == 0;
		} }
		bool IsDefaultPromethean { get {
			return Mode == 1 ? IsDefaultPrometheanMode1 : IsDefaultPrometheanMode0;
		} }

		public bool IsDefaultMode0 { get {
			return Mode == 0 && IsDefaultPrometheanMode0 && unk2C1 == false;
		} }
		public bool IsDefaultMode1 { get {
			return Mode == 1 && IsDefaultPrometheanMode1 && unk2C1 == false;
		} }
		#endregion

		public /*virtual*/ void RevertToDefault(byte mode)
		{
			Mode = mode;

			if (Mode == 1)
				PrometheanEnergyKill = PrometheanEnergyTime = PrometheanEnergyMedal = 3;
			else
				PrometheanEnergyKill = PrometheanEnergyTime = PrometheanEnergyMedal = 0;

			PrometheanDuration = 0;
			unk2C1 = false;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Mode, 2);
			s.Stream(ref PrometheanEnergyKill, 3);
			s.Stream(ref PrometheanEnergyTime, 3);
			s.Stream(ref PrometheanEnergyMedal, 3);
			s.Stream(ref PrometheanDuration, 4);
			s.Stream(ref unk2C1);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("mode", ref Mode);
			using (var bm = s.EnterCursorBookmarkOpt("Promethean", this, o=>!o.IsDefaultPromethean)) if (bm.IsNotNull)
			{
				s.StreamAttribute("energyKill", ref PrometheanEnergyKill);
				s.StreamAttribute("energyTime", ref PrometheanEnergyTime);
				s.StreamAttribute("energyMedal", ref PrometheanEnergyMedal);
				s.StreamAttributeOpt("duration", ref PrometheanDuration, Predicates.IsNotZero);
			}
			else
				RevertToDefault(Mode);

			s.StreamAttributeOpt("unk2C1", ref unk2C1, Predicates.IsTrue);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class GameEngineBaseVariantHalo4
		: Blam.RuntimeData.Variants.GameEngineBaseVariant
	{
		public GameOptionsPrototype OptionsPrototype { get; private set; }

		RequisitionData OptionsRequisitions { get; /*private*/ set; }
		int unk33E4;

		public GameOptionsOrdnanceOptions OrdnanceOptions { get; private set; }

		public GameEngineBaseVariantHalo4(Blam.RuntimeData.Variants.GameEngineVariant variantManager) : base(variantManager)
		{
			OptionsMisc = new GameOptionsMiscHalo4();
			OptionsPrototype = new GameOptionsPrototype();
			OptionsRespawning = new GameOptionsRepawningHalo4(this);

			OptionsMapOverrides = new GameOptionsMapOverridesHalo4(this);
			OptionsRequisitions = new RequisitionData();
			unk33E4 = TypeExtensions.kNone;
			TeamOptions = new GameOptionsTeamOptionsHalo4(variantManager.GameBuild);
			LoadoutOptions = new GameOptionsLoadoutsHalo4();
			OrdnanceOptions = new GameOptionsOrdnanceOptions();
		}

		#region IBitStreamSerializable Members
		public override void Serialize(IO.BitStream s)
		{
			s.StreamObject(Header);									// 0x4
			SerializeFlags(s, 2);									// 0x33E8
			s.StreamObject(OptionsMisc);							// 0x2B4
			s.StreamObject(OptionsPrototype);						// 0x2BC
			s.StreamObject(OptionsRespawning);						// 0x2C4
			s.StreamObject(OptionsSocial);
			s.StreamObject(OptionsMapOverrides);					// 0x3D4
			s.StreamObject(OptionsRequisitions);					// 0x26D8
			s.Stream(ref unk33E4);
			Contract.Assert(unk33E4 == -1); // haven't see it equal anything but -1
			s.StreamObject(TeamOptions);							// 0xD00
			s.StreamObject(LoadoutOptions);							// 0x2084
			s.StreamObject(OrdnanceOptions);						// 0x21B4
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);

			if (!s.StreamAttributeOpt("unk33E4", ref unk33E4, Predicates.IsNotNone))
				unk33E4 = -1;

			SerializeContentHeader(s);
			SerializeMiscOptions(s);
			using (var bm = s.EnterCursorBookmarkOpt("Prototype", OptionsPrototype, obj=>!obj.IsEmpty)) if(bm.IsNotNull)
				s.StreamObject(OptionsPrototype);
			SerializRespawnOptions(s);

			SerializeSocialOptions(s);
			SerializMapOverrides(s);
			using (var bm = s.EnterCursorBookmarkOpt("Requisitions", OptionsRequisitions, obj=>!obj.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(OptionsRequisitions);
			SerializeTeams(s);

			SerializeLoadoutOptions(s);
			using (var bm = s.EnterCursorBookmarkOpt("Ordnance", OrdnanceOptions, obj => !obj.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(OrdnanceOptions);
			else if(s.IsReading)
				OrdnanceOptions.RevertToDefault();
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class RequisitionItem
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public int PaletteIndex;
		bool unk0_bit0;
		int unk8;
		public int MaxInstances;
		float unk14;
		public int ModelVariantName,
			StartingAmmo;
		float unk20;
		uint unk24, unk28, unk2C;
		public byte MaxBuyPlayer, MaxBuyTeam;

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref PaletteIndex, 7);
			s.Stream(ref unk0_bit0);
			s.Stream(ref unk8);
			s.Stream(ref MaxInstances, 30);
			s.Stream(ref unk14);
			s.Stream(ref ModelVariantName, 30);
			s.Stream(ref StartingAmmo, 30);
			s.Stream(ref unk20);
			s.Stream(ref unk24);
			s.Stream(ref unk28);
			s.Stream(ref unk2C);
			s.Stream(ref MaxBuyPlayer);
			s.Stream(ref MaxBuyTeam);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			throw new NotImplementedException();
		}
		#endregion
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class RequisitionData
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		float unk26D8;
		int unk26DC;
		List<RequisitionItem> RequisitionItems { get; /*private*/ set; }

		public bool IsDefault { get {
			return unk26D8 == 0.0f && unk26DC == 0 && RequisitionItems.Count == 0;
		} }

		public RequisitionData()
		{
			RequisitionItems = new List<RequisitionItem>();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref unk26D8);
			s.Stream(ref unk26DC);
			s.StreamElements(RequisitionItems, 7);					// 0x26E0	0x26E4
			Contract.Assert(RequisitionItems.Count == 0);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("unk26D8", ref unk26D8, Predicates.IsNotZero);
			s.StreamAttributeOpt("unk26DC", ref unk26DC, Predicates.IsNotZero);

			using(var bm = s.EnterCursorBookmarkOpt("Items", RequisitionItems, Predicates.HasItems)) if(bm.IsNotNull)
				s.StreamableElements("entry", RequisitionItems);
		}
		#endregion
	};
}