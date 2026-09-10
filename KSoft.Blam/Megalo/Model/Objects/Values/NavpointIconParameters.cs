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
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public MegaloScriptNavpointIconType IconType {
			get { return mIconType; }
			set { mIconType = value;
				NotifyPropertyChanged(kIconTypeChangedEventArgs);
		} }
		#endregion
		#region Numeric
		MegaloScriptVariableReferenceData mNumeric = MegaloScriptVariableReferenceData.Custom;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public MegaloScriptVariableReferenceData Numeric {
			get { return mNumeric; }
			set { mNumeric = value;
				NotifyPropertyChanged(kNumericChangedEventArgs);
		} }
		#endregion

		public MegaloScriptNavpointIconParametersValue(MegaloScriptValueType valueType) : base(valueType)
		{
			ThrowIfUnexpectedBaseType(valueType, MegaloScriptValueBaseType.NavpointIconParameters);
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
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptNavpointIconParametersValue>(other);

			return mIconType == obj.mIconType && mNumeric.Equals(obj.mNumeric);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mIconType, 5, NavpointIconTypeBitStreamer.Instance);
			if (mIconType == MegaloScriptNavpointIconType.Territory)
			{
				mNumeric.SerializeCustom(model, s);
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			if (!s.StreamAttributeEnumOpt("iconType", ref mIconType, e => e != MegaloScriptNavpointIconType.None))
			{
				mIconType = MegaloScriptNavpointIconType.None;
			}

			if (mIconType == MegaloScriptNavpointIconType.Territory)
			{
				using (s.EnterCursorBookmark("TerritoryDesignator"))
				{
					mNumeric.SerializeCustom(model, s);
				}
			}
		}
		#endregion
	};
}
