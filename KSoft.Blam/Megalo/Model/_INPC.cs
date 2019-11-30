using System.Collections.Specialized;
using System.ComponentModel;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptAccessibleObjectBase
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		protected void NotifyPropertiesChanged(PropertyChangedEventArgs[] argsList, int startIndex = 0)
		{
			PropertyChanged.SafeNotify(this, argsList, startIndex);
		}
		#endregion

		static readonly PropertyChangedEventArgs kCodeNameChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptAccessibleObjectBase x) => x.CodeName);
	};
}

namespace KSoft.Blam.Megalo
{
	partial class MegaloScriptGameStatistic
	{
		static readonly PropertyChangedEventArgs kNameStringIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameStatistic x) => x.NameStringIndex);
		static readonly PropertyChangedEventArgs kFormatChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameStatistic x) => x.Format);
		static readonly PropertyChangedEventArgs kSortOrderChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameStatistic x) => x.SortOrder);
		static readonly PropertyChangedEventArgs kGroupingChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameStatistic x) => x.Grouping);

		static readonly PropertyChangedEventArgs kUnk5Changed =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameStatistic x) => x.Unk5);
		static readonly PropertyChangedEventArgs kIsScoreToWinChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameStatistic x) => x.IsScoreToWin);
	};
}

