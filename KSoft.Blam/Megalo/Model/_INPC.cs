using System.Collections.Specialized;
using System.ComponentModel;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion
	};

	partial class MegaloScriptAccessibleObjectBase
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		protected void NotifyPropertiesChanged(PropertyChangedEventArgs[] argsList, int startIndex = 0)
		{
			PropertyChanged.SafeNotify(this, argsList, startIndex);
		}
		#endregion
	};

	#region MegaloScriptModelObject
	partial class MegaloScriptModelObject
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion
	};

	#endregion

	partial class MegaloScriptArguments
		: INotifyCollectionChanged
	{
		#region INotifyCollectionChanged
		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		protected void NotifyItemsInitialized()
		{
			CollectionChanged.SafeNotify(this, ObjectModel.Util.kNotifyCollectionReset);
		}
		protected void NotifyItemChanged(int index, int oldValueId, int newValueId)
		{
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
				oldValueId, newValueId, index));
		}
		#endregion
	};

	#region MegaloScriptCondition
	partial class MegaloScriptUnionGroup
		: INotifyCollectionChanged
	{
		#region INotifyCollectionChanged
		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		void NotifyItemChanged(int index, MegaloScriptModelObjectHandle oldValue, MegaloScriptModelObjectHandle newValue)
		{
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
				oldValue, newValue, index));
		}
		void NotifyItemsSwapped(int lhsValueIndex, MegaloScriptModelObjectHandle lhsValue,
			int rhsValueIndex, MegaloScriptModelObjectHandle rhsValue)
		{
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
				lhsValue, rhsValueIndex, lhsValueIndex)); // index: new <- old
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
				rhsValue, lhsValueIndex, rhsValueIndex)); // index: new <- old
		}
		void NotifyItemInserted(int index, MegaloScriptModelObjectHandle value)
		{
			NotifyPropertyChanged(kCountChangedEventArgs);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				value, index));
		}
		void NotifyItemRemoved(int index, MegaloScriptModelObjectHandle value)
		{
			NotifyPropertyChanged(kCountChangedEventArgs);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
				value, index));
		}
		#endregion
	};

	#endregion

	partial class MegaloScriptConditionActionReferences
		: INotifyCollectionChanged
		, INotifyPropertyChanged
	{
		#region INotifyCollectionChanged
		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		void NotifyItemsInitialized()
		{
			CollectionChanged.SafeNotify(this, ObjectModel.Util.kNotifyCollectionReset);
		}
		void NotifyItemChanged(int index, MegaloScriptModelObjectHandle oldValue, MegaloScriptModelObjectHandle newValue)
		{
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
				oldValue, newValue, index));
		}
		void NotifyItemsSwapped(int lhsValueIndex, MegaloScriptModelObjectHandle lhsValue,
			int rhsValueIndex, MegaloScriptModelObjectHandle rhsValue)
		{
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
				lhsValue, rhsValueIndex, lhsValueIndex)); // index: new <- old
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
				rhsValue, lhsValueIndex, rhsValueIndex)); // index: new <- old
		}
		void NotifyItemInserted(int index, MegaloScriptModelObjectHandle value)
		{
			NotifyPropertyChanged(kCountChangedEventArgs);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				value, index));
		}
		void NotifyItemRemoved(int index, MegaloScriptModelObjectHandle value)
		{
			NotifyPropertyChanged(kCountChangedEventArgs);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
				value, index));
		}
		#endregion

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion
	};

	#region Triggers
	partial class MegaloScriptTrigger
	{
		static readonly PropertyChangedEventArgs kCodeNameChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((IMegaloScriptAccessibleObject x) => x.CodeName);
	};
	#endregion

	partial class MegaloScriptToken
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion
	};
	#region Values
	#endregion

	partial class MegaloScriptObjectFilter
	{
		static readonly PropertyChangedEventArgs[] kParameterChanged = [
			kHasObjectTypeIndexChangedEventArgs,
			kHasTeamChangedEventArgs,
			kHasNumericChangedEventArgs,
		];
	};

}
