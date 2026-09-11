
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
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mLabelStringIndex), AlwaysNotify = true)]
		public partial int LabelStringIndex { get; set; }
		#endregion
		#region Unknown1
		int mUnknown1;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mUnknown1), AlwaysNotify = true)]
		public partial int Unknown1 { get; set; }
		#endregion
		#region Unknown2
		int mUnknown2;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mUnknown2), AlwaysNotify = true)]
		public partial int Unknown2 { get; set; }
		#endregion
		#region Unknown3
		int mUnknown3;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mUnknown3), AlwaysNotify = true)]
		public partial int Unknown3 { get; set; }
		#endregion

		#region Type
		MegaloScriptGameObjectType mType;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mType), AlwaysNotify = true)]
		public partial MegaloScriptGameObjectType Type { get; set; }
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