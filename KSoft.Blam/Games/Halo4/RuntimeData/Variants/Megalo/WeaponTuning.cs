using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	using BarrelModifiersBitStreamer = IO.EnumBitStreamer<WeaponTuningBarrelModifiers>;

	using WeaponModifiersBitStreamer = IO.EnumBitStreamer<WeaponTuningModifiers>;

	// sub_823CECD8
	public sealed partial class WeaponTuningBarrelModifierData
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		WeaponTuningBarrelModifiers Modifiers;
		short mValue0, mValue1, mValue2, mValue3, mValue4, mValue5, mValue6,
			mValue7, mValue8, mValue9, mValue10;
		float mValue11, mValue12, mValue13, mValue14, mValue15, mValue16;
		short mValue17, mValue18;
		float mValue19, mValue20, mValue21, mValue22, mValue23, mValue24;
		short mValue25, mValue26;

		public bool IsUnchanged { get { return Modifiers == 0; } }

		public void Clear()
		{
			Modifiers = 0;
		}

		#region Values
		T? GetImpl<T>(T value, WeaponTuningBarrelModifiers modifer)
			where T : struct
		{
			return EnumFlags.Test(Modifiers, modifer) ? value : (T?)null;
		}
		void SetImpl<T>(T? newValue, ref T value, WeaponTuningBarrelModifiers modifer)
			where T : struct
		{
			var old_flags = Modifiers;
			int index = Bits.TrailingZerosCount((uint)modifer);

			EnumFlags.Modify(newValue.HasValue, ref Modifiers, modifer);
			value = newValue.GetValueOrDefault();

			if (old_flags != Modifiers)
				NotifyPropertyChanged(kIsUnchangedChanged);

			NotifyPropertyChanged(kValueChanged[index]);
		}

		public short? Value0 {
			get { return GetImpl(mValue0, WeaponTuningBarrelModifiers.Value0); }
			set { SetImpl(value, ref mValue0, WeaponTuningBarrelModifiers.Value0); }
		}
		public short? Value1 {
			get { return GetImpl(mValue1, WeaponTuningBarrelModifiers.Value1); }
			set { SetImpl(Value1, ref mValue1, WeaponTuningBarrelModifiers.Value1); }
		}
		public short? Value2 {
			get { return GetImpl(mValue2, WeaponTuningBarrelModifiers.Value2); }
			set { SetImpl(Value2, ref mValue2, WeaponTuningBarrelModifiers.Value2); }
		}
		public short? Value3 {
			get { return GetImpl(mValue3, WeaponTuningBarrelModifiers.Value3); }
			set { SetImpl(Value3, ref mValue3, WeaponTuningBarrelModifiers.Value3); }
		}
		public short? Value4 {
			get { return GetImpl(mValue4, WeaponTuningBarrelModifiers.Value4); }
			set { SetImpl(Value4, ref mValue4, WeaponTuningBarrelModifiers.Value4); }
		}
		public short? Value5 {
			get { return GetImpl(mValue5, WeaponTuningBarrelModifiers.Value5); }
			set { SetImpl(Value5, ref mValue5, WeaponTuningBarrelModifiers.Value5); }
		}
		public short? Value6 {
			get { return GetImpl(mValue6, WeaponTuningBarrelModifiers.Value6); }
			set { SetImpl(Value6, ref mValue6, WeaponTuningBarrelModifiers.Value6); }
		}
		public short? Value7 {
			get { return GetImpl(mValue7, WeaponTuningBarrelModifiers.Value7); }
			set { SetImpl(Value7, ref mValue7, WeaponTuningBarrelModifiers.Value7); }
		}
		public short? Value8 {
			get { return GetImpl(mValue8, WeaponTuningBarrelModifiers.Value8); }
			set { SetImpl(Value8, ref mValue8, WeaponTuningBarrelModifiers.Value8); }
		}
		public short? Value9 {
			get { return GetImpl(mValue9, WeaponTuningBarrelModifiers.Value9); }
			set { SetImpl(Value9, ref mValue9, WeaponTuningBarrelModifiers.Value9); }
		}
		public short? Value10 {
			get { return GetImpl(mValue10, WeaponTuningBarrelModifiers.Value10); }
			set { SetImpl(Value10, ref mValue10, WeaponTuningBarrelModifiers.Value10); }
		}
		public float? Value11 {
			get { return GetImpl(mValue11, WeaponTuningBarrelModifiers.Value11); }
			set { SetImpl(Value11, ref mValue11, WeaponTuningBarrelModifiers.Value11); }
		}
		public float? Value12 {
			get { return GetImpl(mValue12, WeaponTuningBarrelModifiers.Value12); }
			set { SetImpl(Value12, ref mValue12, WeaponTuningBarrelModifiers.Value12); }
		}
		public float? Value13 {
			get { return GetImpl(mValue13, WeaponTuningBarrelModifiers.Value13); }
			set { SetImpl(Value13, ref mValue13, WeaponTuningBarrelModifiers.Value13); }
		}
		public float? Value14 {
			get { return GetImpl(mValue14, WeaponTuningBarrelModifiers.Value14); }
			set { SetImpl(Value14, ref mValue14, WeaponTuningBarrelModifiers.Value14); }
		}
		public float? Value15 {
			get { return GetImpl(mValue15, WeaponTuningBarrelModifiers.Value15); }
			set { SetImpl(Value15, ref mValue15, WeaponTuningBarrelModifiers.Value15); }
		}
		public float? Value16 {
			get { return GetImpl(mValue16, WeaponTuningBarrelModifiers.Value16); }
			set { SetImpl(Value16, ref mValue16, WeaponTuningBarrelModifiers.Value16); }
		}
		public short? Value17 {
			get { return GetImpl(mValue17, WeaponTuningBarrelModifiers.Value17); }
			set { SetImpl(Value17, ref mValue17, WeaponTuningBarrelModifiers.Value17); }
		}
		public short? Value18 {
			get { return GetImpl(mValue18, WeaponTuningBarrelModifiers.Value18); }
			set { SetImpl(Value18, ref mValue18, WeaponTuningBarrelModifiers.Value18); }
		}
		public float? Value19 {
			get { return GetImpl(mValue19, WeaponTuningBarrelModifiers.Value19); }
			set { SetImpl(Value19, ref mValue19, WeaponTuningBarrelModifiers.Value19); }
		}
		public float? Value20 {
			get { return GetImpl(mValue20, WeaponTuningBarrelModifiers.Value20); }
			set { SetImpl(Value20, ref mValue20, WeaponTuningBarrelModifiers.Value20); }
		}
		public float? Value21 {
			get { return GetImpl(mValue21, WeaponTuningBarrelModifiers.Value21); }
			set { SetImpl(Value21, ref mValue21, WeaponTuningBarrelModifiers.Value21); }
		}
		public float? Value22 {
			get { return GetImpl(mValue22, WeaponTuningBarrelModifiers.Value22); }
			set { SetImpl(Value22, ref mValue22, WeaponTuningBarrelModifiers.Value22); }
		}
		public float? Value23 {
			get { return GetImpl(mValue23, WeaponTuningBarrelModifiers.Value23); }
			set { SetImpl(Value23, ref mValue23, WeaponTuningBarrelModifiers.Value23); }
		}
		public float? Value24 {
			get { return GetImpl(mValue24, WeaponTuningBarrelModifiers.Value24); }
			set { SetImpl(Value24, ref mValue24, WeaponTuningBarrelModifiers.Value24); }
		}
		public short? Value25 {
			get { return GetImpl(mValue25, WeaponTuningBarrelModifiers.Value25); }
			set { SetImpl(Value25, ref mValue25, WeaponTuningBarrelModifiers.Value25); }
		}
		public short? Value26 {
			get { return GetImpl(mValue26, WeaponTuningBarrelModifiers.Value26); }
			set { SetImpl(Value26, ref mValue26, WeaponTuningBarrelModifiers.Value26); }
		}
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Modifiers, Bits.kInt64BitCount, BarrelModifiersBitStreamer.Instance);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value0)) s.Stream(ref mValue0);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value1)) s.Stream(ref mValue1);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value2)) s.Stream(ref mValue2);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value3)) s.Stream(ref mValue3);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value4)) s.Stream(ref mValue4, 7);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value5)) s.Stream(ref mValue5, 7);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value6)) s.Stream(ref mValue6);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value7)) s.Stream(ref mValue7);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value8)) s.Stream(ref mValue8);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value9)) s.Stream(ref mValue9);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value10)) s.Stream(ref mValue10);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value11)) s.Stream(ref mValue11, 0f, 130f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value12)) s.Stream(ref mValue12, 0f, 130f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value13)) s.Stream(ref mValue13, 0f, 10f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value14)) s.Stream(ref mValue14, 0f, 6.2831855f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value15)) s.Stream(ref mValue15, 0f, 6.2831855f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value16)) s.Stream(ref mValue16, 0f, 6.2831855f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value17)) s.Stream(ref mValue17);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value18)) s.Stream(ref mValue18);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value19)) s.Stream(ref mValue19, 0f, 10f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value20)) s.Stream(ref mValue20, 0f, 3000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value21)) s.Stream(ref mValue21, 0f, 3000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value22)) s.Stream(ref mValue22, 0f, 3000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value23)) s.Stream(ref mValue23, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value24)) s.Stream(ref mValue24, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value25)) s.Stream(ref mValue25);
			if (EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value26)) s.Stream(ref mValue26);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			#region Value0
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value0) || reading) &&
				s.StreamElementOpt("Value0", ref mValue0))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value0);
			#endregion
			#region Value1
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value1) || reading) &&
				s.StreamElementOpt("Value1", ref mValue1))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value1);
			#endregion
			#region Value2
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value2) || reading) &&
				s.StreamElementOpt("Value2", ref mValue2))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value2);
			#endregion
			#region Value3
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value3) || reading) &&
				s.StreamElementOpt("Value3", ref mValue3))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value3);
			#endregion
			#region Value4
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value4) || reading) &&
				s.StreamElementOpt("Value4", ref mValue4))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value4);
			#endregion
			#region Value5
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value5) || reading) &&
				s.StreamElementOpt("Value5", ref mValue5))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value5);
			#endregion
			#region Value6
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value6) || reading) &&
				s.StreamElementOpt("Value6", ref mValue6))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value6);
			#endregion
			#region Value7
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value7) || reading) &&
				s.StreamElementOpt("Value7", ref mValue7))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value7);
			#endregion
			#region Value8
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value8) || reading) &&
				s.StreamElementOpt("Value8", ref mValue8))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value8);
			#endregion
			#region Value9
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value9) || reading) &&
				s.StreamElementOpt("Value9", ref mValue9))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value9);
			#endregion
			#region Value10
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value10) || reading) &&
				s.StreamElementOpt("Value10", ref mValue10))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value10);
			#endregion
			#region Value11
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value11) || reading) &&
				s.StreamElementOpt("Value11", ref mValue11))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value11);
			#endregion
			#region Value12
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value12) || reading) &&
				s.StreamElementOpt("Value12", ref mValue12))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value12);
			#endregion
			#region Value13
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value13) || reading) &&
				s.StreamElementOpt("Value13", ref mValue13))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value13);
			#endregion
			#region Value14
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value14) || reading) &&
				s.StreamElementOpt("Value14", ref mValue14))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value14);
			#endregion
			#region Value15
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value15) || reading) &&
				s.StreamElementOpt("Value15", ref mValue15))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value15);
			#endregion
			#region Value16
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value16) || reading) &&
				s.StreamElementOpt("Value16", ref mValue16))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value16);
			#endregion
			#region Value17
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value17) || reading) &&
				s.StreamElementOpt("Value17", ref mValue17))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value17);
			#endregion
			#region Value18
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value18) || reading) &&
				s.StreamElementOpt("Value18", ref mValue18))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value18);
			#endregion
			#region Value19
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value19) || reading) &&
				s.StreamElementOpt("Value19", ref mValue19))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value19);
			#endregion
			#region Value20
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value20) || reading) &&
				s.StreamElementOpt("Value20", ref mValue20))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value20);
			#endregion
			#region Value21
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value21) || reading) &&
				s.StreamElementOpt("Value21", ref mValue21))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value21);
			#endregion
			#region Value22
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value22) || reading) &&
				s.StreamElementOpt("Value22", ref mValue22))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value22);
			#endregion
			#region Value23
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value23) || reading) &&
				s.StreamElementOpt("Value23", ref mValue23))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value23);
			#endregion
			#region Value24
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value24) || reading) &&
				s.StreamElementOpt("Value24", ref mValue24))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value24);
			#endregion
			#region Value25
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value25) || reading) &&
				s.StreamElementOpt("Value25", ref mValue25))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value25);
			#endregion
			#region Value26
			if ((EnumFlags.Test(Modifiers, WeaponTuningBarrelModifiers.Value26) || reading) &&
				s.StreamElementOpt("Value26", ref mValue26))
				EnumFlags.Add(ref Modifiers, WeaponTuningBarrelModifiers.Value26);
			#endregion
		}
		#endregion
	};

	// sub_823CF6D8
	public sealed partial class WeaponTuningWeaponModifierData
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		WeaponTuningModifiers Modifiers;
		float mValue0, mValue1, mValue2, mValue3, mValue4, mValue5, mValue6,
			mValue7, mValue8, mValue9, mValue10, mValue11, mValue12, mValue13;

		public bool IsUnchanged { get { return Modifiers == 0; } }

		public void Clear()
		{
			Modifiers = 0;
		}

		#region Values
		T? GetImpl<T>(T value, WeaponTuningModifiers modifer)
			where T : struct
		{
			return EnumFlags.Test(Modifiers, modifer) ? value : (T?)null;
		}
		void SetImpl<T>(T? newValue, ref T value, WeaponTuningModifiers modifer)
			where T : struct
		{
			var old_flags = Modifiers;
			int index = Bits.TrailingZerosCount((uint)modifer);

			EnumFlags.Modify(newValue.HasValue, ref Modifiers, modifer);
			value = newValue.GetValueOrDefault();

			if (old_flags != Modifiers)
				NotifyPropertyChanged(kIsUnchangedChanged);

			NotifyPropertyChanged(kValueChanged[index]);
		}

		public float? Value0 {
			get { return GetImpl(mValue0, WeaponTuningModifiers.Value0); }
			set { SetImpl(value, ref mValue0, WeaponTuningModifiers.Value0); }
		}
		public float? Value1 {
			get { return GetImpl(mValue1, WeaponTuningModifiers.Value1); }
			set { SetImpl(Value1, ref mValue1, WeaponTuningModifiers.Value1); }
		}
		public float? Value2 {
			get { return GetImpl(mValue2, WeaponTuningModifiers.Value2); }
			set { SetImpl(Value2, ref mValue2, WeaponTuningModifiers.Value2); }
		}
		public float? Value3 {
			get { return GetImpl(mValue3, WeaponTuningModifiers.Value3); }
			set { SetImpl(Value3, ref mValue3, WeaponTuningModifiers.Value3); }
		}
		public float? Value4 {
			get { return GetImpl(mValue4, WeaponTuningModifiers.Value4); }
			set { SetImpl(Value4, ref mValue4, WeaponTuningModifiers.Value4); }
		}
		public float? Value5 {
			get { return GetImpl(mValue5, WeaponTuningModifiers.Value5); }
			set { SetImpl(Value5, ref mValue5, WeaponTuningModifiers.Value5); }
		}
		public float? Value6 {
			get { return GetImpl(mValue6, WeaponTuningModifiers.Value6); }
			set { SetImpl(Value6, ref mValue6, WeaponTuningModifiers.Value6); }
		}
		public float? Value7 {
			get { return GetImpl(mValue7, WeaponTuningModifiers.Value7); }
			set { SetImpl(Value7, ref mValue7, WeaponTuningModifiers.Value7); }
		}
		public float? Value8 {
			get { return GetImpl(mValue8, WeaponTuningModifiers.Value8); }
			set { SetImpl(Value8, ref mValue8, WeaponTuningModifiers.Value8); }
		}
		public float? Value9 {
			get { return GetImpl(mValue9, WeaponTuningModifiers.Value9); }
			set { SetImpl(Value9, ref mValue9, WeaponTuningModifiers.Value9); }
		}
		public float? Value10 {
			get { return GetImpl(mValue10, WeaponTuningModifiers.Value10); }
			set { SetImpl(Value10, ref mValue10, WeaponTuningModifiers.Value10); }
		}
		public float? Value11 {
			get { return GetImpl(mValue11, WeaponTuningModifiers.Value11); }
			set { SetImpl(Value11, ref mValue11, WeaponTuningModifiers.Value11); }
		}
		public float? Value12 {
			get { return GetImpl(mValue12, WeaponTuningModifiers.Value12); }
			set { SetImpl(Value12, ref mValue12, WeaponTuningModifiers.Value12); }
		}
		public float? Value13 {
			get { return GetImpl(mValue13, WeaponTuningModifiers.Value13); }
			set { SetImpl(Value13, ref mValue13, WeaponTuningModifiers.Value13); }
		}
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Modifiers, Bits.kInt64BitCount, WeaponModifiersBitStreamer.Instance);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value0)) s.Stream(ref mValue0, 0f, 1f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value1)) s.Stream(ref mValue1, 0f, 1f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value2)) s.Stream(ref mValue2, 0f, 1f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value3)) s.Stream(ref mValue3, 0f, 1f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value4)) s.Stream(ref mValue4, 0f, 1f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value5)) s.Stream(ref mValue5, 0f, 1.5706964f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value6)) s.Stream(ref mValue6, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value7)) s.Stream(ref mValue7, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value8)) s.Stream(ref mValue8, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value9)) s.Stream(ref mValue9, 0f, 1.5706964f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value10)) s.Stream(ref mValue10, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value11)) s.Stream(ref mValue11, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value12)) s.Stream(ref mValue12, 0f, 1000f, 20, true, true);
			if (EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value13)) s.Stream(ref mValue13, 0f, 6.2831855f, 20, true, true);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			#region Value0
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value0) || reading) &&
				s.StreamElementOpt("Value0", ref mValue0))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value0);
			#endregion
			#region Value1
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value1) || reading) &&
				s.StreamElementOpt("Value1", ref mValue1))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value1);
			#endregion
			#region Value2
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value2) || reading) &&
				s.StreamElementOpt("Value2", ref mValue2))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value2);
			#endregion
			#region Value3
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value3) || reading) &&
				s.StreamElementOpt("Value3", ref mValue3))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value3);
			#endregion
			#region Value4
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value4) || reading) &&
				s.StreamElementOpt("Value4", ref mValue4))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value4);
			#endregion
			#region Value5
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value5) || reading) &&
				s.StreamElementOpt("Value5", ref mValue5))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value5);
			#endregion
			#region Value6
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value6) || reading) &&
				s.StreamElementOpt("Value6", ref mValue6))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value6);
			#endregion
			#region Value7
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value7) || reading) &&
				s.StreamElementOpt("Value7", ref mValue7))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value7);
			#endregion
			#region Value8
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value8) || reading) &&
				s.StreamElementOpt("Value8", ref mValue8))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value8);
			#endregion
			#region Value9
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value9) || reading) &&
				s.StreamElementOpt("Value9", ref mValue9))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value9);
			#endregion
			#region Value10
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value10) || reading) &&
				s.StreamElementOpt("Value10", ref mValue10))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value10);
			#endregion
			#region Value11
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value11) || reading) &&
				s.StreamElementOpt("Value11", ref mValue11))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value11);
			#endregion
			#region Value12
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value12) || reading) &&
				s.StreamElementOpt("Value12", ref mValue12))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value12);
			#endregion
			#region Value13
			if ((EnumFlags.Test(Modifiers, WeaponTuningModifiers.Value13) || reading) &&
				s.StreamElementOpt("Value13", ref mValue13))
				EnumFlags.Add(ref Modifiers, WeaponTuningModifiers.Value13);
			#endregion
		}
		#endregion
	};

	// 0x1AE44
	public sealed partial class WeaponTuningBarrelsModifierData
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public WeaponTuningBarrelModifierData Primary = new WeaponTuningBarrelModifierData();
		public WeaponTuningBarrelModifierData Secondary = new WeaponTuningBarrelModifierData();

		public bool IsUnchanged { get { return Primary.IsUnchanged && Secondary.IsUnchanged; } }

		public void Clear()
		{
			Primary.Clear();
			Secondary.Clear();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			Primary.Serialize(s);
			Secondary.Serialize(s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Primary", Primary, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				Primary.Serialize(s);
			using (var bm = s.EnterCursorBookmarkOpt("Secondary", Secondary, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				Secondary.Serialize(s);
		}
		#endregion
	};

	public sealed partial class WeaponTuningData
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region kDamageReportingTypeToTuningEntry
#if false
		enum TuningTableType
		{
			OneBarrel,
			TwoBarrels,
		};
		struct DamageReportingTypeToTuningMap
		{
			public static readonly DamageReportingTypeToTuningMap None = new DamageReportingTypeToTuningMap(-1, 0);

			public short Index;
			public TuningTableType Table;

			public DamageReportingTypeToTuningMap(short index, TuningTableType table)
			{
				Index = index;
				Table = table;
			}
		};
		static readonly List<DamageReportingTypeToTuningMap> kDamageReportingTypeToTuningEntry;
#endif
		#endregion
		#region kOneBarrelEntries
		static readonly DamageReportingType[] kOneBarrelEntries = new DamageReportingType[] {
			DamageReportingType.BurstGun, // suppressor
			DamageReportingType.SpreadGun, // scattershot
			DamageReportingType.AttachBeam, // binary_rifle
			DamageReportingType.ScaleshotRifle,//IncinerationCannon
			DamageReportingType.MagnumPistol,
			DamageReportingType.AssaultRifle,
			DamageReportingType.MarksmanRifle, // DMR
			DamageReportingType.Shotgun,
			DamageReportingType.BattleRifle,
			DamageReportingType.SniperRifle,
			DamageReportingType.RocketLauncher,
			DamageReportingType.StickyGrenadeLauncher,
			DamageReportingType.LMG,
			DamageReportingType.Needler,
			DamageReportingType.Carbine,
			DamageReportingType.BeamRifle,
			DamageReportingType.StormRifle,
			DamageReportingType.ConcussionRifle,
			DamageReportingType.FlakCannon, // FRG
			DamageReportingType.Ghost,
			DamageReportingType.Wraith,
			DamageReportingType.WraithAntiInfantry,
			DamageReportingType.Banshee,
			DamageReportingType.BansheeBomb,
			DamageReportingType.WarthogGunner,
			DamageReportingType.WarthogGunner_Gauss,
			DamageReportingType.WarthogGunner_Rocket,
			DamageReportingType.Scorpion,
			DamageReportingType.ScorpionGunner, // turret
			DamageReportingType.MechChaingun,
			DamageReportingType.MechRocket,
			DamageReportingType.MagnumPistol_CTF,
			DamageReportingType.FloodTalons,
		};
		#endregion
		#region kTwoBarrelsEntries
		static readonly DamageReportingType[] kTwoBarrelsEntries = new DamageReportingType[] {
			DamageReportingType.HomingRifle, // light rifle
			DamageReportingType.StatisGun, // bolt shot
			DamageReportingType.SpartanLaser,
			DamageReportingType.RailGun,
			DamageReportingType.PlasmaPistol,
			DamageReportingType.PersonalAutoTurret,
		};
		#endregion

		static readonly string[] kUnk000_Names = new string[6];
		static readonly string[] kUnk408_Names = new string[33];
		static readonly string[] kUnk109E_Names = new string[33];
		static readonly string[] kUnkF1E_Names = new string[6];

		static WeaponTuningData()
		{
			Contract.Assert(kOneBarrelEntries.Length == 33);
			Contract.Assert(kTwoBarrelsEntries.Length == 6);
			#region kDamageReportingTypeToTuningEntry
#if false
			kDamageReportingTypeToTuningEntry = new List<DamageReportingTypeToTuningMap>((int)DamageReportingType.kNumberOf);
			for (int x = 0; x < kDamageReportingTypeToTuningEntry.Capacity; x++)
				kDamageReportingTypeToTuningEntry.Add(DamageReportingTypeToTuningMap.None);

			for (int x = 0; x < kOneBarrelEntries.Length; x++)
				kDamageReportingTypeToTuningEntry[(int)kOneBarrelEntries[x]] = new DamageReportingTypeToTuningMap((short)x, TuningTableType.OneBarrel);
			for (int x = 0; x < kTwoBarrelsEntries.Length; x++)
				kDamageReportingTypeToTuningEntry[(int)kTwoBarrelsEntries[x]] = new DamageReportingTypeToTuningMap((short)x, TuningTableType.TwoBarrels);
#endif
			#endregion

#if false
			for (int x = 0; x < kUnk000_Names.Length; x++)	kUnk000_Names[x] = "Unk000_" + x.ToString();
			for (int x = 0; x < kUnk408_Names.Length; x++)	kUnk408_Names[x] = "Unk408_" + x.ToString();
			for (int x = 0; x < kUnk109E_Names.Length; x++)	kUnk109E_Names[x] = "Unk109E_" + x.ToString();
			for (int x = 0; x < kUnkF1E_Names.Length; x++)	kUnkF1E_Names[x] = "UnkF1E_" + x.ToString();
#else
			for (int x = 0; x < kUnk000_Names.Length; x++) kUnk000_Names[x] = kTwoBarrelsEntries[x].ToString();
			for (int x = 0; x < kUnk408_Names.Length; x++) kUnk408_Names[x] = kOneBarrelEntries[x].ToString();
			for (int x = 0; x < kUnk109E_Names.Length; x++) kUnk109E_Names[x] = kOneBarrelEntries[x].ToString();
			for (int x = 0; x < kUnkF1E_Names.Length; x++) kUnkF1E_Names[x] = kTwoBarrelsEntries[x].ToString();
#endif
		}

		public WeaponTuningBarrelsModifierData[] Unk000 = new WeaponTuningBarrelsModifierData[6];
		public WeaponTuningBarrelModifierData[] Unk408 = new WeaponTuningBarrelModifierData[33];
		public WeaponTuningWeaponModifierData[] Unk109E = new WeaponTuningWeaponModifierData[33];
		public WeaponTuningWeaponModifierData[] UnkF1E = new WeaponTuningWeaponModifierData[6];

		#region IsUnchanged
		public bool Unk000_IsUnchanged { get { return Array.TrueForAll(Unk000, unk => unk.IsUnchanged); } }
		public bool Unk408_IsUnchanged { get { return Array.TrueForAll(Unk408, unk => unk.IsUnchanged); } }
		public bool Unk109E_IsUnchanged { get { return Array.TrueForAll(Unk109E, unk => unk.IsUnchanged); } }
		public bool UnkF1E_IsUnchanged { get { return Array.TrueForAll(UnkF1E, unk => unk.IsUnchanged); } }
		public bool IsUnchanged { get { return Unk000_IsUnchanged && Unk408_IsUnchanged && Unk109E_IsUnchanged && UnkF1E_IsUnchanged; } }
		#endregion

		public WeaponTuningData()
		{
			for (int x = 0; x < Unk000.Length; x++)	Unk000[x] = new WeaponTuningBarrelsModifierData();
			for (int x = 0; x < Unk408.Length; x++)	Unk408[x] = new WeaponTuningBarrelModifierData();
			for (int x = 0; x < Unk109E.Length; x++)Unk109E[x] = new WeaponTuningWeaponModifierData();
			for (int x = 0; x < UnkF1E.Length; x++)	UnkF1E[x] = new WeaponTuningWeaponModifierData();
		}

		public void Clear()
		{
			foreach (var obj in Unk000) obj.Clear();
			foreach (var obj in Unk408) obj.Clear();
			foreach (var obj in Unk109E) obj.Clear();
			foreach (var obj in UnkF1E) obj.Clear();
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			for (int x = 0; x < Unk000.Length; x++)
				Unk000[x].Serialize(s);
			for (int x = 0; x < Unk408.Length; x++)
				Unk408[x].Serialize(s);
			for (int x = 0; x < Unk109E.Length; x++)
				Unk109E[x].Serialize(s);
			for (int x = 0; x < UnkF1E.Length; x++)
				UnkF1E[x].Serialize(s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("TwoBarrels", this, obj=>!obj.Unk000_IsUnchanged)) if (bm.IsNotNull)
				for (int x = 0; x < Unk000.Length; x++)
					using (var bm2 = s.EnterCursorBookmarkOpt(kUnk000_Names[x], Unk000[x], obj=>!obj.IsUnchanged)) if(bm2.IsNotNull)
						Unk000[x].Serialize(s);

			using (var bm = s.EnterCursorBookmarkOpt("OneBarrel", this, obj=>!obj.Unk408_IsUnchanged)) if (bm.IsNotNull)
				for (int x = 0; x < Unk408.Length; x++)
					using (var bm2 = s.EnterCursorBookmarkOpt(kUnk408_Names[x], Unk408[x], obj=>!obj.IsUnchanged)) if (bm2.IsNotNull)
						Unk408[x].Serialize(s);

			using (var bm = s.EnterCursorBookmarkOpt("OneBarrelWeapons", this, obj=>!obj.Unk109E_IsUnchanged)) if (bm.IsNotNull)
				for (int x = 0; x < Unk109E.Length; x++)
					using (var bm2 = s.EnterCursorBookmarkOpt(kUnk109E_Names[x], Unk109E[x], obj=>!obj.IsUnchanged)) if (bm2.IsNotNull)
						Unk109E[x].Serialize(s);

			using (var bm = s.EnterCursorBookmarkOpt("TwoBarrelWeapons", this, obj=>!obj.UnkF1E_IsUnchanged)) if (bm.IsNotNull)
				for (int x = 0; x < UnkF1E.Length; x++)
					using (var bm2 = s.EnterCursorBookmarkOpt(kUnkF1E_Names[x], UnkF1E[x], obj=>!obj.IsUnchanged)) if (bm2.IsNotNull)
						UnkF1E[x].Serialize(s);
		}
		#endregion
	};
}