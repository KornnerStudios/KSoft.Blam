using System.Collections.Specialized;
using System.ComponentModel;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs kInitializationTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.InitializationTriggerIndex);
		static readonly PropertyChangedEventArgs kLocalInitializationTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.LocalInitializationTriggerIndex);
		static readonly PropertyChangedEventArgs kHostMigrationTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.HostMigrationTriggerIndex);
		static readonly PropertyChangedEventArgs kDoubleHostMigrationTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.DoubleHostMigrationTriggerIndex);
		static readonly PropertyChangedEventArgs kObjectDeathEventTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.ObjectDeathEventTriggerIndex);
		static readonly PropertyChangedEventArgs kLocalTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.LocalTriggerIndex);
		static readonly PropertyChangedEventArgs kPregameTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.PregameTriggerIndex);
		static readonly PropertyChangedEventArgs kIncidentTriggerIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModel x) => x.IncidentTriggerIndex);
	};

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

	#region MegaloScriptModelObject
	partial class MegaloScriptModelObject
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion
	};

	partial class MegaloScriptModelNamedObject
	{
		static readonly PropertyChangedEventArgs kNameChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModelNamedObject x) => x.Name);
	};

	partial class MegaloScriptModelObjectWithParameters
	{
		static readonly PropertyChangedEventArgs kCommentOutChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModelObjectWithParameters x) => x.CommentOut);
		static readonly PropertyChangedEventArgs kArgumentsChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptModelObjectWithParameters x) => x.Arguments);
	};
	#endregion

	partial class MegaloScriptArguments
		: INotifyCollectionChanged
	{
		#region INotifyCollectionChanged
		public event NotifyCollectionChangedEventHandler CollectionChanged;

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
		public event NotifyCollectionChangedEventHandler CollectionChanged;

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

	partial class MegaloScriptCondition
	{
		static readonly PropertyChangedEventArgs kProtoDataChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptCondition x) => x.ProtoData);

		static readonly PropertyChangedEventArgs kInvertedChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptCondition x) => x.Inverted);
		static readonly PropertyChangedEventArgs kUnionGroupChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptCondition x) => x.UnionGroup);
	};
	#endregion

	partial class MegaloScriptAction
	{
		static readonly PropertyChangedEventArgs kProtoDataChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptAction x) => x.ProtoData);
	}

	partial class MegaloScriptConditionActionReferences
		: INotifyCollectionChanged
		, INotifyPropertyChanged
	{
		#region INotifyCollectionChanged
		public event NotifyCollectionChangedEventHandler CollectionChanged;

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
		public event PropertyChangedEventHandler PropertyChanged;

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
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs kTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptToken x) => x.Type);
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptToken x) => x.Value);
	};
	#region Values
	partial class MegaloScriptValueBase
	{
		static readonly PropertyChangedEventArgs kIsGlobalChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptValueBase x) => x.IsGlobal);
	};

	partial class MegaloScriptBoolValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptBoolValue x) => x.Value);
	};

	partial class MegaloScriptIntValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptIntValue x) => x.Value);
	};
	partial class MegaloScriptUIntValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptUIntValue x) => x.Value);
	};
	partial class MegaloScriptSingleValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptSingleValue x) => x.Value);
	};
	partial class MegaloScriptPoint3dValue
	{
		static readonly PropertyChangedEventArgs kXChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptPoint3dValue x) => x.X);
		static readonly PropertyChangedEventArgs kYChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptPoint3dValue x) => x.Y);
		static readonly PropertyChangedEventArgs kZChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptPoint3dValue x) => x.Z);
	};

	partial class MegaloScriptFlagsValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptFlagsValue x) => x.Value);
	};
	partial class MegaloScriptEnumValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptEnumValue x) => x.Value);
	};
	partial class MegaloScriptIndexValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptIndexValue x) => x.Value);
	};

	partial class MegaloScriptVarIndexValue
	{
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptVarIndexValue x) => x.Value);
	};
	partial class MegaloScriptVarReferenceValueBase
	{
		static readonly PropertyChangedEventArgs kVarChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptVarReferenceValueBase x) => x.Var);
	};
	partial class MegaloScriptTokensValue
	{
		static readonly PropertyChangedEventArgs kStringIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTokensValue x) => x.StringIndex);
		static readonly PropertyChangedEventArgs kTokenCountChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTokensValue x) => x.TokenCount);
	};

	partial class MegaloScriptVirtualTriggerValue
	{
		static readonly PropertyChangedEventArgs kVirtualTriggerHandleChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptVirtualTriggerValue x) => x.VirtualTriggerHandle);
	};
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
	partial class MegaloScriptTargetVarValue
	{
		static readonly PropertyChangedEventArgs kTargetTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTargetVarValue x) => x.TargetType);
		static readonly PropertyChangedEventArgs kValueChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTargetVarValue x) => x.Value);
	};
	partial class MegaloScriptTeamFilterParametersValue
	{
		static readonly PropertyChangedEventArgs kFilterTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTeamFilterParametersValue x) => x.FilterType);
		static readonly PropertyChangedEventArgs kPlayerChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTeamFilterParametersValue x) => x.Player);
		static readonly PropertyChangedEventArgs kPlayerAddOrRemoveChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTeamFilterParametersValue x) => x.PlayerAddOrRemove);
	};
	partial class MegaloScriptNavpointIconParametersValue
	{
		static readonly PropertyChangedEventArgs kIconTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptNavpointIconParametersValue x) => x.IconType);
		static readonly PropertyChangedEventArgs kNumericChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptNavpointIconParametersValue x) => x.Numeric);
	};
	partial class MegaloScriptWidgetMeterParametersValue
	{
		static readonly PropertyChangedEventArgs kTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptWidgetMeterParametersValue x) => x.Type);
		static readonly PropertyChangedEventArgs kTimerChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptWidgetMeterParametersValue x) => x.Timer);
		static readonly PropertyChangedEventArgs kNumeric1Changed =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptWidgetMeterParametersValue x) => x.Numeric1);
		static readonly PropertyChangedEventArgs kNumeric2Changed =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptWidgetMeterParametersValue x) => x.Numeric2);
	};
	partial class MegaloScriptObjectReferenceWithPlayerVarIndexValue
	{
		static readonly PropertyChangedEventArgs kPlayerVarIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectReferenceWithPlayerVarIndexValue x) => x.PlayerVarIndex);
	};
	#endregion

	#region Variables
	partial class MegaloScriptVariableBase
	{
		static readonly PropertyChangedEventArgs kNetworkStateChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptVariableBase x) => x.NetworkState);
		static readonly PropertyChangedEventArgs kUnknownChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptVariableBase x) => x.Unknown);
	};
	partial class MegaloScriptVariableWithVarReferenceBase
	{
		static readonly PropertyChangedEventArgs kVarChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptVariableWithVarReferenceBase x) => x.Var);
	};

	partial class MegaloScriptTeamVariable
	{
		static readonly PropertyChangedEventArgs kTeamDesignatorChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptTeamVariable x) => x.TeamDesignator);
	};
	#endregion

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

	partial class MegaloScriptHudWidget
	{
		static readonly PropertyChangedEventArgs kPositionChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptHudWidget x) => x.Position);
	};

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
		static readonly PropertyChangedEventArgs[] kParameterChanged = new PropertyChangedEventArgs[] {
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.HasObjectTypeIndex),
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.HasNumeric),
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptObjectFilter x) => x.HasNumeric),
		};
	};

	partial class MegaloScriptGameObjectFilter
	{
		static readonly PropertyChangedEventArgs kLabelStringIndexChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameObjectFilter x) => x.LabelStringIndex);
		static readonly PropertyChangedEventArgs kUnknown1Changed =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameObjectFilter x) => x.Unknown1);
		static readonly PropertyChangedEventArgs kUnknown2Changed =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameObjectFilter x) => x.Unknown2);
		static readonly PropertyChangedEventArgs kUnknown3Changed =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameObjectFilter x) => x.Unknown3);

		static readonly PropertyChangedEventArgs kTypeChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptGameObjectFilter x) => x.Type);
	};
}

