
namespace KSoft.Blam.Megalo.Model
{
	using MegaloScriptObjectFilterValidParametersBitStreamer = IO.EnumBitStreamer<MegaloScriptObjectFilterValidParameters>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptObjectFilter
		: Model.MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region LabelStringIndex
		int mLabelStringIndex; // 0x0
		public int LabelStringIndex {
			get { return mLabelStringIndex; }
			set { mLabelStringIndex = value;
				NotifyPropertyChanged(kLabelStringIndexChanged);
		} }
		#endregion

		MegaloScriptObjectFilterValidParameters mValidParameters;	// 0xB

		#region ObjectTypeIndex
		int mObjectTypeIndex; // 0x4
		public int ObjectTypeIndex {
			get { return mObjectTypeIndex; }
			set { mObjectTypeIndex = value;
				NotifyPropertyChanged(kObjectTypeIndexChanged);
		} }
		#endregion
		#region Team
		int mTeam; // 0xA sbyte at runtime
		public int Team {
			get { return mTeam; }
			set { mTeam = value;
				NotifyPropertyChanged(kTeamChanged);
		} }
		#endregion
		#region Numeric
		int mNumeric; // 0x8 short at runtime
		public int Numeric {
			get { return mNumeric; }
			set { mNumeric = value;
				NotifyPropertyChanged(kNumericChanged);
		} }
		#endregion

		#region Minimum
		int mMinimum; // 0xC short at runtime
		public int Minimum {
			get { return mMinimum; }
			set { mMinimum = value;
				NotifyPropertyChanged(kMinimumChanged);
		} }
		#endregion

		#region ValidParameters interface
		public bool HasParameters { get { return mValidParameters != 0; } }

		void ParameterSet(bool value, MegaloScriptObjectFilterValidParameters param)
		{
			EnumFlags.Modify(value, ref mValidParameters, param);

			int index = Bits.TrailingZerosCount((uint)param);
			NotifyPropertyChanged(kParameterChanged[index]);
			NotifyPropertyChanged(kHasParametersChanged);
		}
		public bool HasObjectTypeIndex	{
			get { return EnumFlags.Test(mValidParameters, MegaloScriptObjectFilterValidParameters.ObjectType); }
			set { ParameterSet(value, MegaloScriptObjectFilterValidParameters.ObjectType); }
		}
		public bool HasTeam	{
			get { return EnumFlags.Test(mValidParameters, MegaloScriptObjectFilterValidParameters.Team); }
			set { ParameterSet(value, MegaloScriptObjectFilterValidParameters.Team); }
		}
		public bool HasNumeric	{
			get { return EnumFlags.Test(mValidParameters, MegaloScriptObjectFilterValidParameters.Numeric); }
			set { ParameterSet(value, MegaloScriptObjectFilterValidParameters.Numeric); }
		}
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			var model = (Model.MegaloScriptModel)s.Owner;

			model.MegaloVariant.StreamStringTableIndexPointer(s, ref mLabelStringIndex);
			s.Stream(ref mValidParameters, 3, MegaloScriptObjectFilterValidParametersBitStreamer.Instance);
			if (HasObjectTypeIndex)	model.Database.StreamObjectTypeIndex(s, ref mObjectTypeIndex);
			if (HasTeam)			Model.MegaloScriptEnumValue.SerializeValue(model, s, model.Database.TeamDesignatorValueType, ref mTeam);
			if (HasNumeric)			s.Stream(ref mNumeric, 16);

			s.Stream(ref mMinimum, 7);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var model = (Model.MegaloScriptModel)s.Owner;

			model.MegaloVariant.SerializeStringTableIndexOpt(s, "labelIndex", ref mLabelStringIndex);
			SerializeCodeName(s);
			s.StreamAttributeEnumOpt("params", ref mValidParameters, f => f != 0, true);
			s.StreamAttributeOpt("min", ref mMinimum, Predicates.IsNotZero);
			if (HasObjectTypeIndex)
			{
				Model.MegaloScriptIndexValue.SerializeValue(model, s, model.Database.ObjectTypeIndexValueType,
					ref mObjectTypeIndex, IO.TagElementNodeType.Element, "ObjectType");
			}
			if (HasTeam)
			{
				Model.MegaloScriptEnumValue.SerializeValue(model, s, model.Database.TeamDesignatorValueType,
					ref mTeam, IO.TagElementNodeType.Element, "Team");
			}
			if (HasNumeric)
				s.StreamElement("Numeric", ref mNumeric);
		}
		#endregion
	};
}