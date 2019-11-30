
namespace KSoft.Blam.Megalo
{
	using StatFormatBitStreamer = IO.EnumBitStreamer
		< MegaloScriptGameStatisticFormat
		>;
	using StatSortOrderBitStreamer = IO.EnumBitStreamerWithOptions
		< MegaloScriptGameStatisticSortOrder
		, IO.EnumBitStreamerOptions.ShouldUseNoneSentinelEncoding
		>;
	using StatGroupingBitStreamer = IO.EnumBitStreamer
		< MegaloScriptGameStatisticGrouping
		>;

	namespace Model
	{
		partial class MegaloScriptModel
		{
			protected virtual MegaloScriptGameStatistic NewGameStatistic()
			{
				return new MegaloScriptGameStatistic();
			}
		};
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptGameStatistic
		: Model.MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region NameStringIndex
		int mNameStringIndex; // sbyte
		public int NameStringIndex {
			get { return mNameStringIndex; }
			set { mNameStringIndex = value;
				NotifyPropertyChanged(kNameStringIndexChanged);
		} }
		#endregion
		#region Format
		MegaloScriptGameStatisticFormat mFormat; // byte
		public MegaloScriptGameStatisticFormat Format {
			get { return mFormat; }
			set { mFormat = value;
				NotifyPropertyChanged(kFormatChanged);
		} }
		#endregion
		#region SortOrder
		MegaloScriptGameStatisticSortOrder mSortOrder; // sbyte
		public MegaloScriptGameStatisticSortOrder SortOrder {
			get { return mSortOrder; }
			set { mSortOrder = value;
				NotifyPropertyChanged(kSortOrderChanged);
		} }
		#endregion
		#region Grouping
		MegaloScriptGameStatisticGrouping mGrouping; // sbyte
		public MegaloScriptGameStatisticGrouping Grouping {
			get { return mGrouping; }
			set { mGrouping = value;
				NotifyPropertyChanged(kGroupingChanged);
		} }
		#endregion

		// Halo4
		#region Unk5
		bool mUnk5;
		public bool Unk5 {
			get { return mUnk5; }
			set { mUnk5 = value;
				NotifyPropertyChanged(kUnk5Changed);
		} }

		public virtual bool SupportsUnk5 { get { return false; } }
		#endregion
		#region IsScoreToWin
		bool mIsScoreToWin; // true if this is the stat used for determining score-to-win
		public bool IsScoreToWin {
			get { return mIsScoreToWin; }
			set { mIsScoreToWin = value;
				NotifyPropertyChanged(kIsScoreToWinChanged);
		} }

		public virtual bool SupportsIsScoreToWin { get { return false; } }
		#endregion

		#region IBitStreamSerializable Members
		public virtual void Serialize(IO.BitStream s)
		{
			var model = (Model.MegaloScriptModel)s.Owner;

#if false // #TODO_BLAM_REFACTOR
			model.MegaloVariant.StreamStringTableIndexReference(s, ref mNameStringIndex);
#endif
			s.Stream(ref mFormat, 2, StatFormatBitStreamer.Instance);
			s.Stream(ref mSortOrder, 2, StatSortOrderBitStreamer.Instance);
			s.Stream(ref mGrouping, 1, StatGroupingBitStreamer.Instance);
			if (SupportsUnk5) s.Stream(ref mUnk5);
			if (SupportsIsScoreToWin) s.Stream(ref mIsScoreToWin);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var model = (Model.MegaloScriptModel)s.Owner;

#if false // #TODO_BLAM_REFACTOR
			model.MegaloVariant.SerializeStringTableIndexOpt(s, "nameIndex", ref mNameStringIndex);
#endif
			s.StreamAttributeEnum("format", ref mFormat);
			s.StreamAttributeEnumOpt("sortOrder", ref mSortOrder, e => e != MegaloScriptGameStatisticSortOrder.Ascending);
			s.StreamAttributeEnumOpt("grouping", ref mGrouping, e => e != 0);

			SerializeCodeName(s);

			if (SupportsUnk5) s.StreamAttributeOpt("unk5", ref mUnk5, Predicates.IsTrue);
			if (SupportsIsScoreToWin) s.StreamAttributeOpt("isScoreToWin", ref mIsScoreToWin, Predicates.IsTrue);
		}
		#endregion
	};
}

