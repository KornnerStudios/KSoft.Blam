using System.Collections.Specialized;
using System.ComponentModel;

namespace KSoft.Blam.RuntimeData.Variants
{
	partial class OptionalRealArray
		: INotifyPropertyChanged
		, INotifyCollectionChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		#region INotifyCollectionChanged
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		void NotifyItemsInitialized()
		{
			CollectionChanged.SafeNotify(this, ObjectModel.Util.kNotifyCollectionReset);
		}
		void NotifyItemChanged(int index, float? oldValue, float? newValue)
		{
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
				oldValue, newValue, index));
		}
		#endregion

		static readonly PropertyChangedEventArgs kHasValuesChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((OptionalRealArray x) => x.HasValues);
	};
}