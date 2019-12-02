#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	using NavpointIconTypeBitStreamer = IO.EnumBitStreamerWithOptions
		< MegaloScriptNavpointIconType
		, IO.EnumBitStreamerOptions.ShouldUseNoneSentinelEncoding
		>;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {IconType}")]
	public sealed partial class MegaloScriptNavpointIconParametersValue
		: MegaloScriptValueBase
	{
		#region IconType
		MegaloScriptNavpointIconType mIconType;
		public MegaloScriptNavpointIconType IconType {
			get { return mIconType; }
			set { mIconType = value;
				NotifyPropertyChanged(kIconTypeChanged);
		} }
		#endregion
		#region Numeric
		MegaloScriptVariableReferenceData mNumeric = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData Numeric {
			get { return mNumeric; }
			set { mNumeric = value;
				NotifyPropertyChanged(kNumericChanged);
		} }
		#endregion

		public MegaloScriptNavpointIconParametersValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.NavpointIconParameters);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptNavpointIconParametersValue)model.CreateValue(ValueType);
			result.IconType = IconType;
			result.Numeric = Numeric;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptNavpointIconParametersValue)other;

			return mIconType == obj.mIconType && mNumeric.Equals(obj.mNumeric);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mIconType, 5, NavpointIconTypeBitStreamer.Instance);
			if (mIconType == MegaloScriptNavpointIconType.Territory)
				mNumeric.SerializeCustom(model, s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			if (!s.StreamAttributeEnumOpt("iconType", ref mIconType, e => e != MegaloScriptNavpointIconType.None))
				mIconType = MegaloScriptNavpointIconType.None;

			if (mIconType == MegaloScriptNavpointIconType.Territory)
				using (s.EnterCursorBookmark("TerritoryDesignator"))
					mNumeric.SerializeCustom(model, s);
		}
		#endregion
	};
}