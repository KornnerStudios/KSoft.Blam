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
			NotifyPropertyChanged(kCountChanged);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				value, index));
		}
		void NotifyItemRemoved(int index, MegaloScriptModelObjectHandle value)
		{
			NotifyPropertyChanged(kCountChanged);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
				value, index));
		}
		#endregion

		static readonly PropertyChangedEventArgs kCountChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptConditionActionReferences x) => x.Count);
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
			NotifyPropertyChanged(kCountChanged);
			CollectionChanged.SafeNotify(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				value, index));
		}
		void NotifyItemRemoved(int index, MegaloScriptModelObjectHandle value)
		{
			NotifyPropertyChanged(kCountChanged);
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

		static readonly PropertyChangedEventArgs kCountChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptConditionActionReferences x) => x.Count);
	};

	#region Triggers
	partial class MegaloScriptTrigger
	{
		static readonly PropertyChangedEventArgs kCodeNameChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((IMegaloScriptAccessibleObject x) => x.CodeName);

		static readonly PropertyChangedEventArgs kExecutionModeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.ExecutionMode);
		static readonly PropertyChangedEventArgs kTriggerTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.TriggerType);
		static readonly PropertyChangedEventArgs kObjectFilterIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.ObjectFilterIndex);
		static readonly PropertyChangedEventArgs kGameObjectTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.GameObjectType);
		static readonly PropertyChangedEventArgs kGameObjectFilterIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.GameObjectFilterIndex);
		static readonly PropertyChangedEventArgs kFrameUpdateFrequencyChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.FrameUpdateFrequency);
		static readonly PropertyChangedEventArgs kFrameUpdateOffsetChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.FrameUpdateOffset);

		static readonly PropertyChangedEventArgs kCommentOutChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTrigger x) => x.CommentOut);
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
	partial class MegaloScriptShapeValue
	{
		static readonly PropertyChangedEventArgs kShapeTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptShapeValue x) => x.ShapeType);

		static readonly PropertyChangedEventArgs kRadiusChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptShapeValue x) => x.Radius);
		static readonly PropertyChangedEventArgs kLengthChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptShapeValue x) => x.Length);
		static readonly PropertyChangedEventArgs kTopChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptShapeValue x) => x.Top);
		static readonly PropertyChangedEventArgs kBottomChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptShapeValue x) => x.Bottom);
	};
	#endregion

	partial class MegaloScriptObjectFilter
	{
		static readonly PropertyChangedEventArgs kLabelStringIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.LabelStringIndex);

		static readonly PropertyChangedEventArgs kObjectTypeIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.ObjectTypeIndex);
		static readonly PropertyChangedEventArgs kTeamChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.Team);
		static readonly PropertyChangedEventArgs kNumericChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.Numeric);

		static readonly PropertyChangedEventArgs kMinimumChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.Minimum);

		static readonly PropertyChangedEventArgs kHasParametersChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.HasParameters);
		static readonly PropertyChangedEventArgs[] kParameterChanged = [
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.HasObjectTypeIndex),
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.HasNumeric),
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.HasNumeric),
		];
	};

}
