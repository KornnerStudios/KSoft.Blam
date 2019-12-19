using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	public enum GameOptionsPrototypeMode
	{
		NotNewVersion,
		IsNewVersion,

		kNumberOf
	};

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
		public bool ClassColorOverride;

		public bool IsEmpty { get {
			return Mode == (int)GameOptionsPrototypeMode.NotNewVersion && PrometheanEnergyKill == 0 && PrometheanEnergyTime == 0 &&
				PrometheanEnergyMedal == 0 && PrometheanDuration == 0 && ClassColorOverride == false;
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
			return Mode == (int)GameOptionsPrototypeMode.IsNewVersion ? IsDefaultPrometheanMode1 : IsDefaultPrometheanMode0;
		} }

		public bool IsDefaultMode0 { get {
			return Mode == (int)GameOptionsPrototypeMode.NotNewVersion && IsDefaultPrometheanMode0 && ClassColorOverride == false;
		} }
		public bool IsDefaultMode1 { get {
			return Mode == (int)GameOptionsPrototypeMode.IsNewVersion && IsDefaultPrometheanMode1 && ClassColorOverride == false;
		} }
		#endregion

		public /*virtual*/ void RevertToDefault(byte mode)
		{
			Mode = mode;

			if (Mode == (int)GameOptionsPrototypeMode.IsNewVersion)
				PrometheanEnergyKill = PrometheanEnergyTime = PrometheanEnergyMedal = 3;
			else
				PrometheanEnergyKill = PrometheanEnergyTime = PrometheanEnergyMedal = 0;

			PrometheanDuration = 0;
			ClassColorOverride = false;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Mode, 2);
			s.Stream(ref PrometheanEnergyKill, 3);
			s.Stream(ref PrometheanEnergyTime, 3);
			s.Stream(ref PrometheanEnergyMedal, 3);
			s.Stream(ref PrometheanDuration, 4);
			s.Stream(ref ClassColorOverride);
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

			s.StreamAttributeOpt("classColorOverride", ref ClassColorOverride, Predicates.IsTrue);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class GameEngineBaseVariantHalo4
		: Blam.RuntimeData.Variants.GameEngineBaseVariant
	{
		public GameOptionsPrototype OptionsPrototype { get; private set; }

		RequisitionData OptionsRequisitions { get; /*private*/ set; }
		public int InfinityMissionId;

		public GameOptionsOrdnanceOptions OrdnanceOptions { get; private set; }

		public GameEngineBaseVariantHalo4(Blam.RuntimeData.Variants.GameEngineVariant variantManager) : base(variantManager)
		{
			OptionsMisc = new GameOptionsMiscHalo4();
			OptionsPrototype = new GameOptionsPrototype();
			OptionsRespawning = new GameOptionsRepawningHalo4(this);

			OptionsMapOverrides = new GameOptionsMapOverridesHalo4(this);
			OptionsRequisitions = new RequisitionData();
			InfinityMissionId = TypeExtensions.kNone;
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
			s.Stream(ref InfinityMissionId);
			Contract.Assert(InfinityMissionId.IsNone()); // haven't see it equal anything but -1
			s.StreamObject(TeamOptions);							// 0xD00
			s.StreamObject(LoadoutOptions);							// 0x2084
			s.StreamObject(OrdnanceOptions);						// 0x21B4
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnumOpt("flags", ref Flags, flags => flags != 0, true);

			if (!s.StreamAttributeOpt("infinityMissionId", ref InfinityMissionId, Predicates.IsNotNone))
				InfinityMissionId = TypeExtensions.kNone;

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
		public bool Locked;
		public int DesignerId;
		public byte SubMenu;
		public int MaxInstances;
		public float Price;
		public int ModelVariantName,
			StartingAmmo;
		public float WarmUpSeconds;
		public float PurchaseFrequencySecondsPerPlayer;
		public float PurchaseFrequencySecondsPerTeam;
		public float PriceIncreaseFactor;
		public byte MaxBuyPlayer, MaxBuyTeam;

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref PaletteIndex, 7);
			s.Stream(ref Locked);
			s.Stream(ref DesignerId);
			s.Stream(ref SubMenu, 1);
			s.Stream(ref MaxInstances, 30);
			s.Stream(ref Price);
			s.Stream(ref ModelVariantName, 30);
			s.Stream(ref StartingAmmo, 30);
			s.Stream(ref WarmUpSeconds);
			s.Stream(ref PurchaseFrequencySecondsPerPlayer);
			s.Stream(ref PurchaseFrequencySecondsPerTeam);
			s.Stream(ref PriceIncreaseFactor);
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
		public float PlayerRequisitionFrequencySeconds;
		public int InitialGameCurrency;
		public List<RequisitionItem> RequisitionItems { get; /*private*/ set; }

		public bool IsDefault { get {
			return PlayerRequisitionFrequencySeconds == 0.0f && InitialGameCurrency == 0 && RequisitionItems.Count == 0;
		} }

		public RequisitionData()
		{
			RequisitionItems = new List<RequisitionItem>();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref PlayerRequisitionFrequencySeconds);
			s.Stream(ref InitialGameCurrency);
			s.StreamElements(RequisitionItems, 7);					// 0x26E0	0x26E4
			Contract.Assert(RequisitionItems.Count == 0);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("playerRequisitionFrequencySeconds", ref PlayerRequisitionFrequencySeconds, Predicates.IsNotZero);
			s.StreamAttributeOpt("initialGameCurrency", ref InitialGameCurrency, Predicates.IsNotZero);

			using(var bm = s.EnterCursorBookmarkOpt("Items", RequisitionItems, Predicates.HasItems)) if(bm.IsNotNull)
				s.StreamableElements("entry", RequisitionItems);
		}
		#endregion
	};
}