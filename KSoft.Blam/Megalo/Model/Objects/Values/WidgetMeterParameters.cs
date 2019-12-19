#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	using MegaloScriptWidgetMeterTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptWidgetMeterType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {Type}")]
	public sealed partial class MegaloScriptWidgetMeterParametersValue
		: MegaloScriptValueBase
	{
		#region Type
		MegaloScriptWidgetMeterType mType;
		public MegaloScriptWidgetMeterType Type {
			get { return mType; }
			set { mType = value;
				NotifyPropertyChanged(kTypeChanged);
		} }
		#endregion
		#region Timer
		MegaloScriptVariableReferenceData mTimer = MegaloScriptVariableReferenceData.Timer;
		public MegaloScriptVariableReferenceData Timer {
			get { return mTimer; }
			set { mTimer = value;
				NotifyPropertyChanged(kTimerChanged);
		} }
		#endregion
		#region Numeric1 (fill amount numerator)
		MegaloScriptVariableReferenceData mNumeric1 = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData Numeric1 {
			get { return mNumeric1; }
			set { mNumeric1 = value;
				NotifyPropertyChanged(kNumeric1Changed);
		} }
		#endregion
		#region Numeric2 (fill amount denominator)
		MegaloScriptVariableReferenceData mNumeric2 = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData Numeric2 {
			get { return mNumeric2; }
			set { mNumeric2 = value;
				NotifyPropertyChanged(kNumeric2Changed);
		} }
		#endregion

		public MegaloScriptWidgetMeterParametersValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.WidgetMeterParameters);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptWidgetMeterParametersValue)model.CreateValue(ValueType);
			result.Type = Type;
			result.Timer = Timer;
			result.Numeric1 = Numeric1;
			result.Numeric2 = Numeric2;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptWidgetMeterParametersValue)other;

			bool result = Type == obj.Type;

			if (result)
			{
				if (Type == MegaloScriptWidgetMeterType.Numeric)
					result &= Numeric1.Equals(obj.Numeric1) && Numeric2.Equals(obj.Numeric2);
				else if (Type == MegaloScriptWidgetMeterType.Timer)
					result &= Timer.Equals(obj.Timer);
			}

			return result;
		}

		#region SetAs
		public void SetAsOff()
		{
			Numeric1 = MegaloScriptVariableReferenceData.Custom;
			Numeric2 = MegaloScriptVariableReferenceData.Custom;
			Timer = MegaloScriptVariableReferenceData.Timer;

			Type = MegaloScriptWidgetMeterType.Off;
		}
		public void SetAsNumeric(MegaloScriptVariableReferenceData value, MegaloScriptVariableReferenceData maxValue)
		{
			Contract.Requires(value.ReferenceKind == MegaloScriptVariableReferenceType.Custom);
			Contract.Requires(maxValue.ReferenceKind == MegaloScriptVariableReferenceType.Custom);

			Numeric1 = value;
			Numeric2 = maxValue;
			Timer = MegaloScriptVariableReferenceData.Timer;

			Type = MegaloScriptWidgetMeterType.Numeric;
		}
		public void SetAsTimer(MegaloScriptVariableReferenceData timer)
		{
			Contract.Requires(timer.ReferenceKind == MegaloScriptVariableReferenceType.Timer);

			Numeric1 = MegaloScriptVariableReferenceData.Custom;
			Numeric2 = MegaloScriptVariableReferenceData.Custom;
			Timer = timer;

			Type = MegaloScriptWidgetMeterType.Timer;
		}
		#endregion

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mType, 2, MegaloScriptWidgetMeterTypeBitStreamer.Instance);

			if (Type == MegaloScriptWidgetMeterType.Numeric)
			{
				mNumeric1.SerializeCustom(model, s);
				mNumeric2.SerializeCustom(model, s);
			}
			else if (Type == MegaloScriptWidgetMeterType.Timer)
				mTimer.SerializeTimer(model, s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnum("meterType", ref mType);

			if (Type == MegaloScriptWidgetMeterType.Numeric)
			{
				using (s.EnterCursorBookmark("Value")) mNumeric1.SerializeCustom(model, s);
				using (s.EnterCursorBookmark("MaxValue")) mNumeric2.SerializeCustom(model, s);
			}
			else if (Type == MegaloScriptWidgetMeterType.Timer)
				using (s.EnterCursorBookmark("Timer"))
					mTimer.SerializeTimer(model, s);
		}
		#endregion
	};
}