
namespace KSoft.Blam.Megalo.Model
{
	using MegaloScriptGameObjectTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptGameObjectType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptGameObjectFilter
		: MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region LabelStringIndex
		int mLabelStringIndex; // 0x0
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int LabelStringIndex {
			get { return mLabelStringIndex; }
			set { mLabelStringIndex = value;
				NotifyPropertyChanged(kLabelStringIndexChangedEventArgs);
		} }
		#endregion
		#region Unknown1
		int mUnknown1;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int Unknown1 {
			get { return mUnknown1; }
			set { mUnknown1 = value;
				NotifyPropertyChanged(kUnknown1ChangedEventArgs);
		} }
		#endregion
		#region Unknown2
		int mUnknown2;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int Unknown2 {
			get { return mUnknown2; }
			set { mUnknown2 = value;
				NotifyPropertyChanged(kUnknown2ChangedEventArgs);
		} }
		#endregion
		#region Unknown3
		int mUnknown3;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int Unknown3 {
			get { return mUnknown3; }
			set { mUnknown3 = value;
				NotifyPropertyChanged(kUnknown3ChangedEventArgs);
		} }
		#endregion

		#region Type
		MegaloScriptGameObjectType mType;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public MegaloScriptGameObjectType Type {
			get { return mType; }
			set { mType = value;
				NotifyPropertyChanged(kTypeChangedEventArgs);
		} }
		#endregion

		public MegaloScriptGameObjectFilter()
		{
			mLabelStringIndex = mUnknown1 = mUnknown2 = mUnknown3 = TypeExtensions.kNone;
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s)
		{
			var model = (MegaloScriptModel)s.Owner!;

			model.MegaloVariant.StreamStringTableIndexPointer(s, ref mLabelStringIndex);
			model.MegaloVariant.StreamStringTableIndexPointer(s, ref mUnknown1);
			model.MegaloVariant.StreamStringTableIndexPointer(s, ref mUnknown2);
			model.MegaloVariant.StreamStringTableIndexPointer(s, ref mUnknown3);
			s.Stream(ref mType, 1, MegaloScriptGameObjectTypeBitStreamer.Instance);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var model = (MegaloScriptModel)s.Owner!;

			s.StreamAttributeEnum("type", ref mType);
			model.MegaloVariant.SerializeStringTableIndex(s, "labelIndex", ref mLabelStringIndex);
			model.MegaloVariant.SerializeStringTableIndexOpt(s, "unkIndex1", ref mUnknown1);
			model.MegaloVariant.SerializeStringTableIndexOpt(s, "unkIndex2", ref mUnknown2);
			model.MegaloVariant.SerializeStringTableIndexOpt(s, "unkIndex3", ref mUnknown3);

			SerializeCodeName(s);
		}
		#endregion
	};
}