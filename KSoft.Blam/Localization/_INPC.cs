using System.Collections.Specialized;
using System.ComponentModel;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Localization
{
	partial class LanguageRegistry
	{
		static PropertyChangedEventArgs[] gLanguageChangedEventArgs;
		static void InitializeLanguageChangedEventArgs()
		{
			gLanguageChangedEventArgs = new PropertyChangedEventArgs[NumberOfLanguages];
			for (int x = 0; x < NumberOfLanguages; x++)
				gLanguageChangedEventArgs[x] = new PropertyChangedEventArgs(LanguageNames[x]);
		}

		internal static PropertyChangedEventArgs GetLanguageChangedEventArgs(int langIndex)
		{
			Contract.Requires(langIndex >= 0 && langIndex < NumberOfLanguages);

			if (gLanguageChangedEventArgs == null)
				InitializeLanguageChangedEventArgs();

			Contract.Assume(gLanguageChangedEventArgs != null);
			return gLanguageChangedEventArgs[langIndex];
		}
	};
}

namespace KSoft.Blam.Localization.StringTables
{
	partial class LocaleStringTableReference
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs kCodeNameChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((LocaleStringTableReference x) => x.CodeName);
	};

	partial class LocaleStringTable
		: INotifyCollectionChanged
		, INotifyPropertyChanged
	{
		#region INotifyCollectionChanged
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected void NotifyItemsInitialized()
		{
			NotifyPropertyChanged(kCountChanged);
			CollectionChanged.SafeNotify(this, ObjectModel.Util.kNotifyCollectionReset);
		}
		protected void NotifyItemChanged(int index, LocaleStringTableReference oldValue, LocaleStringTableReference newValue)
		{
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
				oldValue, newValue, index));
		}
		protected void NotifyItemInserted(int index, LocaleStringTableReference value)
		{
			NotifyPropertyChanged(kCountChanged);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				value, index));
		}
		protected void NotifyItemRemoved(int index, LocaleStringTableReference value)
		{
			NotifyPropertyChanged(kCountChanged);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
				value, index));
		}
		#endregion

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs kCountChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((LocaleStringTable x) => x.Count);
	};
}