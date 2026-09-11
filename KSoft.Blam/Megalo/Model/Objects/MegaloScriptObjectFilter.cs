
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
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mLabelStringIndex), AlwaysNotify = true)]
		public partial int LabelStringIndex { get; set; }
		#endregion

		MegaloScriptObjectFilterValidParameters mValidParameters;	// 0xB

		#region ObjectTypeIndex
		int mObjectTypeIndex; // 0x4
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mObjectTypeIndex), AlwaysNotify = true)]
		public partial int ObjectTypeIndex { get; set; }
		#endregion
		#region Team
		int mTeam; // 0xA sbyte at runtime
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mTeam), AlwaysNotify = true)]
		public partial int Team { get; set; }
		#endregion
		#region Numeric
		int mNumeric; // 0x8 short at runtime
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mNumeric), AlwaysNotify = true)]
		public partial int Numeric { get; set; }
		#endregion

		#region Minimum
		int mMinimum; // 0xC short at runtime
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mMinimum), AlwaysNotify = true)]
		public partial int Minimum { get; set; }
		#endregion

		#region ValidParameters interface
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public bool HasParameters { get { return mValidParameters != 0; } }

		void ParameterSet(bool value, MegaloScriptObjectFilterValidParameters param)
		{
			EnumFlags.Modify(value, ref mValidParameters, param);

			int index = System.Numerics.BitOperations.TrailingZeroCount((uint)param);
			NotifyPropertyChanged(kParameterChanged[index]);
			NotifyPropertyChanged(kHasParametersChangedEventArgs);
		}
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public bool HasObjectTypeIndex	{
			get { return mValidParameters.HasFlag(MegaloScriptObjectFilterValidParameters.ObjectType); }
			set { ParameterSet(value, MegaloScriptObjectFilterValidParameters.ObjectType); }
		}
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public bool HasTeam	{
			get { return mValidParameters.HasFlag(MegaloScriptObjectFilterValidParameters.Team); }
			set { ParameterSet(value, MegaloScriptObjectFilterValidParameters.Team); }
		}
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public bool HasNumeric	{
			get { return mValidParameters.HasFlag(MegaloScriptObjectFilterValidParameters.Numeric); }
			set { ParameterSet(value, MegaloScriptObjectFilterValidParameters.Numeric); }
		}
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			var model = (Model.MegaloScriptModel)s.Owner!;

			model.MegaloVariant.StreamStringTableIndexPointer(s, ref mLabelStringIndex);
			s.Stream(ref mValidParameters, 3, MegaloScriptObjectFilterValidParametersBitStreamer.Instance);
			if (HasObjectTypeIndex)
			{
				model.Database.StreamObjectTypeIndex(s, ref mObjectTypeIndex);
			}

			if (HasTeam)
			{
				Model.MegaloScriptEnumValue.SerializeValue(model, s, model.Database.TeamDesignatorValueType, ref mTeam);
			}

			if (HasNumeric)
			{
				s.Stream(ref mNumeric, 16);
			}

			s.Stream(ref mMinimum, 7);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var model = (Model.MegaloScriptModel)s.Owner!;

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
			{
				s.StreamElement("Numeric", ref mNumeric);
			}
		}
		#endregion
	};
}