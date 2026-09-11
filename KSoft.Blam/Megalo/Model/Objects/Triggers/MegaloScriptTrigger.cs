using System;

namespace KSoft.Blam.Megalo.Model
{
	using MegaloScriptTriggerExecutionModeBitStreamer = IO.EnumBitStreamer<MegaloScriptTriggerExecutionMode>;
	using MegaloScriptTriggerTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptTriggerType>;
	using MegaloScriptGameObjectTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptGameObjectType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {TriggerType}, Mode = {ExecutionMode}, Name = {Name}")]
	public abstract partial class MegaloScriptTrigger
		: MegaloScriptTriggerBase
		, IMegaloScriptAccessibleObject
	{
		public override MegaloScriptModelObjectType ObjectType => MegaloScriptModelObjectType.Trigger;

		#region ExecutionMode
		MegaloScriptTriggerExecutionMode mExecutionMode;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mExecutionMode), AlwaysNotify = true)]
		public partial MegaloScriptTriggerExecutionMode ExecutionMode { get; set; }
		#endregion
		#region TriggerType
		MegaloScriptTriggerType mTriggerType;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mTriggerType), AlwaysNotify = true)]
		public partial MegaloScriptTriggerType TriggerType { get; set; }
		#endregion
		#region ObjectFilterIndex
		int mObjectFilterIndex = KSoft.TypeExtensions.kNoneInt32;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mObjectFilterIndex), AlwaysNotify = true)]
		public partial int ObjectFilterIndex { get; set; }
		#endregion
		// Added in Halo4
		#region GameObjectFilter
		MegaloScriptGameObjectType mGameObjectType;
		int mGameObjectFilterIndex = KSoft.TypeExtensions.kNoneInt32;

		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mGameObjectType), AlwaysNotify = true)]
		public partial MegaloScriptGameObjectType GameObjectType { get; set; }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mGameObjectFilterIndex), AlwaysNotify = true)]
		public partial int GameObjectFilterIndex { get; set; }
		#endregion
		#region FrameUpdate
		int mFrameUpdateFrequency;
		int mFrameUpdateOffset;

		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mFrameUpdateFrequency), AlwaysNotify = true)]
		public partial int FrameUpdateFrequency { get; set; }
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mFrameUpdateOffset), AlwaysNotify = true)]
		public partial int FrameUpdateOffset { get; set; }

		public bool HasFrameUpdate { get { return mFrameUpdateFrequency != 0; } }
		#endregion

		#region CommentOut
		bool mCommentOut;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public bool CommentOut {
			get { return mCommentOut; }
			set {
				if (TriggerType != MegaloScriptTriggerType.Normal)
				{
					throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"Only {0} triggers can be commented out; actual trigger type is {1}.",
						MegaloScriptTriggerType.Normal, TriggerType));
				}

				mCommentOut = value;
				NotifyPropertyChanged(kCommentOutChangedEventArgs);
		} }
		#endregion

		#region IBitStreamSerializable Members
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
		protected virtual int kExecutionModeBitLength => 3;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
		protected virtual int kTypeBitLength => 3;

		protected void SerializeFrameUpdate(MegaloScriptModel model, IO.BitStream s)
		{
			Util.MarkUnusedVariable(ref model);

			s.StreamNoneable(ref mFrameUpdateFrequency, 8);
			s.StreamNoneable(ref mFrameUpdateOffset, 8);
		}
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mExecutionMode, kExecutionModeBitLength, MegaloScriptTriggerExecutionModeBitStreamer.Instance);
			s.Stream(ref mTriggerType, kTypeBitLength, MegaloScriptTriggerTypeBitStreamer.Instance);
			if (ExecutionMode == MegaloScriptTriggerExecutionMode.OnObjectFilter)
			{
				model.Database.StreamObjectFilterIndex(s, ref mObjectFilterIndex);
			}
			else if (ExecutionMode == MegaloScriptTriggerExecutionMode.OnCandySpawnerFilter)
			{
				s.Stream(ref mGameObjectType, 1, MegaloScriptGameObjectTypeBitStreamer.Instance);
				model.Database.StreamGameObjectFilterIndex(s, ref mGameObjectFilterIndex);
			}
			References.Serialize(model, s);
		}
		#endregion

		// #NOTE_BLAM: Can be embedded via MegaloScriptModelObjectHandle. Attributes must not conflict with that type's
		#region ITagElementStringNameStreamable Members
		/// <summary>When we're streaming in 'EmbedObjects' mode we only want to write root triggers in the root script node</summary>
		internal static Predicate<MegaloScriptTrigger> SkipIfNotRootPredicate =
			value => value.TriggerType == MegaloScriptTriggerType.InnerLoop;

		protected void SerializeFrameUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("FrameUpdate", this, obj=>obj.HasFrameUpdate))
			{
				if (bm.IsNotNull)
				{
					s.StreamAttribute("frequency", ref mFrameUpdateFrequency);
					s.StreamAttribute("offset", ref mFrameUpdateOffset);
				}
			}

			if (s.IsReading)
			{
				if (mFrameUpdateFrequency < 0)  { mFrameUpdateFrequency = 0; }
				if (mFrameUpdateFrequency == 0) { mFrameUpdateOffset = 0; }
			}
		}
		protected void SerializeReferences<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Elements"))
			{
				References.Serialize(model, s);
			}
		}
		// #NOTE_BLAM: up to concrete implementations to serialize References
		public override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			SerializeIdOpt(model, s);
			s.StreamAttributeEnumOpt("trigType", ref mTriggerType, e => e != MegaloScriptTriggerType.Normal);
			s.StreamAttributeEnumOpt("execMode", ref mExecutionMode, e => e != MegaloScriptTriggerExecutionMode.General);
			SerializeNameOpt(s);

			if (ExecutionMode == MegaloScriptTriggerExecutionMode.OnObjectFilter)
			{
				s.StreamAttributeIdAsString("objectFilter", ref mObjectFilterIndex, model,
					(_model , name) => model.FromIndexName(Proto.MegaloScriptValueIndexTarget.ObjectFilter, name),
					(_model, id) => model.ToIndexName(Proto.MegaloScriptValueIndexTarget.ObjectFilter, id));
			}
			else
			{
				ObjectFilterIndex = -1;
			}

			if (ExecutionMode == MegaloScriptTriggerExecutionMode.OnCandySpawnerFilter)
			{	using (s.EnterCursorBookmark("GameObject"))
				{
					s.StreamAttributeEnum("type", ref mGameObjectType);

					s.StreamAttributeIdAsString("filter", ref mGameObjectFilterIndex, model,
						(_model , name) => model.FromIndexName(Proto.MegaloScriptValueIndexTarget.GameObjectFilter, name),
						(_model, id) => model.ToIndexName(Proto.MegaloScriptValueIndexTarget.GameObjectFilter, id));
				}
			}
			else
			{
				GameObjectFilterIndex = -1;
			}

			if (TriggerType == MegaloScriptTriggerType.Normal)
			{
				s.StreamAttributeOpt("commentOut", ref mCommentOut, Predicates.IsTrue);
			}
		}
		#endregion

		// really only implemented so we can search triggers in the Model's FindNameIndex
		#region IMegaloScriptAccessibleObject Members
		string IMegaloScriptAccessibleObject.CodeName {
			get { return base.Name; }
			set { ArgumentNullException.ThrowIfNull(value);
				base.Name = value;
				NotifyPropertyChanged(kCodeNameChanged);
		} }
		#endregion
	};
}
