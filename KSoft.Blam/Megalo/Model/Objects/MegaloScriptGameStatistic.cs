
namespace KSoft.Blam.Megalo.Model
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

	partial class MegaloScriptModel
	{
		protected virtual MegaloScriptGameStatistic NewGameStatistic()
		{
			return new MegaloScriptGameStatistic();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptGameStatistic
		: MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region NameStringIndex
		int mNameStringIndex; // sbyte
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mNameStringIndex), AlwaysNotify = true)]
		public partial int NameStringIndex { get; set; }
		#endregion
		#region Format
		MegaloScriptGameStatisticFormat mFormat; // byte
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mFormat), AlwaysNotify = true)]
		public partial MegaloScriptGameStatisticFormat Format { get; set; }
		#endregion
		#region SortOrder
		MegaloScriptGameStatisticSortOrder mSortOrder; // sbyte
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mSortOrder), AlwaysNotify = true)]
		public partial MegaloScriptGameStatisticSortOrder SortOrder { get; set; }
		#endregion
		#region Grouping
		MegaloScriptGameStatisticGrouping mGrouping; // sbyte
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mGrouping), AlwaysNotify = true)]
		public partial MegaloScriptGameStatisticGrouping Grouping { get; set; }
		#endregion

		// Halo4
		#region Unk5
		bool mUnk5;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mUnk5), AlwaysNotify = true)]
		public partial bool Unk5 { get; set; }

		public virtual bool SupportsUnk5 { get { return false; } }
		#endregion
		#region IsScoreToWin
		bool mIsScoreToWin; // true if this is the stat used for determining score-to-win
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mIsScoreToWin), AlwaysNotify = true)]
		public partial bool IsScoreToWin { get; set; }

		public virtual bool SupportsIsScoreToWin { get { return false; } }
		#endregion

		#region IBitStreamSerializable Members
		public virtual void Serialize(IO.BitStream s)
		{
			var model = (MegaloScriptModel)s.Owner!;

			model.MegaloVariant.StreamStringTableIndexReference(s, ref mNameStringIndex);
			s.Stream(ref mFormat, 2, StatFormatBitStreamer.Instance);
			s.Stream(ref mSortOrder, 2, StatSortOrderBitStreamer.Instance);
			s.Stream(ref mGrouping, 1, StatGroupingBitStreamer.Instance);
			if (SupportsUnk5)
			{
				s.Stream(ref mUnk5);
			}

			if (SupportsIsScoreToWin)
			{
				s.Stream(ref mIsScoreToWin);
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var model = (MegaloScriptModel)s.Owner!;

			model.MegaloVariant.SerializeStringTableIndexOpt(s, "nameIndex", ref mNameStringIndex);
			s.StreamAttributeEnum("format", ref mFormat);
			s.StreamAttributeEnumOpt("sortOrder", ref mSortOrder, e => e != MegaloScriptGameStatisticSortOrder.Ascending);
			s.StreamAttributeEnumOpt("grouping", ref mGrouping, e => e != 0);

			SerializeCodeName(s);

			if (SupportsUnk5)
			{
				s.StreamAttributeOpt("unk5", ref mUnk5, Predicates.IsTrue);
			}

			if (SupportsIsScoreToWin)
			{
				s.StreamAttributeOpt("isScoreToWin", ref mIsScoreToWin, Predicates.IsTrue);
			}
		}
		#endregion
	};
}