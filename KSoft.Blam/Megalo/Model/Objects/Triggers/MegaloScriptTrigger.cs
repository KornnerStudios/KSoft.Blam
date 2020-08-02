using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

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
		public override MegaloScriptModelObjectType ObjectType { get { return MegaloScriptModelObjectType.Trigger; } }

		#region ExecutionMode
		MegaloScriptTriggerExecutionMode mExecutionMode;
		public MegaloScriptTriggerExecutionMode ExecutionMode {
			get { return mExecutionMode; }
			set { mExecutionMode = value;
				NotifyPropertyChanged(kExecutionModeChanged);
		} }
		#endregion
		#region TriggerType
		MegaloScriptTriggerType mTriggerType;
		public MegaloScriptTriggerType TriggerType {
			get { return mTriggerType; }
			set { mTriggerType = value;
				NotifyPropertyChanged(kTriggerTypeChanged);
		} }
		#endregion
		#region ObjectFilterIndex
		int mObjectFilterIndex = KSoft.TypeExtensions.kNoneInt32;
		public int ObjectFilterIndex {
			get { return mObjectFilterIndex; }
			set { mObjectFilterIndex = value;
				NotifyPropertyChanged(kObjectFilterIndexChanged);
		} }
		#endregion
		// Added in Halo4
		#region GameObjectFilter
		MegaloScriptGameObjectType mGameObjectType;
		int mGameObjectFilterIndex = KSoft.TypeExtensions.kNoneInt32;

		public MegaloScriptGameObjectType GameObjectType {
			get { return mGameObjectType; }
			set { mGameObjectType = value;
				NotifyPropertyChanged(kGameObjectTypeChanged);
		} }
		public int GameObjectFilterIndex {
			get { return mGameObjectFilterIndex; }
			set { mGameObjectFilterIndex = value;
				NotifyPropertyChanged(kGameObjectFilterIndexChanged);
		} }
		#endregion
		#region FrameUpdate
		int mFrameUpdateFrequency;
		int mFrameUpdateOffset;

		public int FrameUpdateFrequency {
			get { return mFrameUpdateFrequency; }
			set { mFrameUpdateFrequency = value;
				NotifyPropertyChanged(kFrameUpdateFrequencyChanged);
		} }
		public int FrameUpdateOffset {
			get { return mFrameUpdateOffset; }
			set { mFrameUpdateOffset = value;
				NotifyPropertyChanged(kFrameUpdateOffsetChanged);
		} }

		public bool HasFrameUpdate { get { return mFrameUpdateFrequency != 0; } }
		#endregion

		#region CommentOut
		bool mCommentOut;
		public bool CommentOut {
			get { return mCommentOut; }
			set { Contract.Requires(TriggerType == MegaloScriptTriggerType.Normal);
				mCommentOut = value;
				NotifyPropertyChanged(kCommentOutChanged);
		} }
		#endregion

		#region IBitStreamSerializable Members
		protected virtual int kExecutionModeBitLength { get { return 3; } }
		protected virtual int kTypeBitLength { get { return 3; } }

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
				model.Database.StreamObjectFilterIndex(s, ref mObjectFilterIndex);
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
			using (var bm = s.EnterCursorBookmarkOpt("FrameUpdate", this, obj=>obj.HasFrameUpdate)) if (bm.IsNotNull)
			{
				s.StreamAttribute("frequency", ref mFrameUpdateFrequency);
				s.StreamAttribute("offset", ref mFrameUpdateOffset);
			}

			if (s.IsReading)
			{
				if (mFrameUpdateFrequency < 0) mFrameUpdateFrequency = 0;
				if (mFrameUpdateFrequency == 0)mFrameUpdateOffset = 0;
			}
		}
		protected void SerializeReferences<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Elements"))
				References.Serialize(model, s);
		}
		// #NOTE_BLAM: up to concrete implementations to serialize References
		public override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			SerializeIdOpt(model, s);
			s.StreamAttributeEnumOpt("trigType", ref mTriggerType, e => e != MegaloScriptTriggerType.Normal);
			s.StreamAttributeEnumOpt("execMode", ref mExecutionMode, e => e != MegaloScriptTriggerExecutionMode.General);
			SerializeNameOpt(s);

			if (ExecutionMode == MegaloScriptTriggerExecutionMode.OnObjectFilter)
				s.StreamAttributeIdAsString("objectFilter", ref mObjectFilterIndex, model,
					(_model , name) => model.FromIndexName(Proto.MegaloScriptValueIndexTarget.ObjectFilter, name),
					(_model, id) => model.ToIndexName(Proto.MegaloScriptValueIndexTarget.ObjectFilter, id));
			else
				ObjectFilterIndex = -1;

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
				GameObjectFilterIndex = -1;

			if(TriggerType == MegaloScriptTriggerType.Normal)
				s.StreamAttributeOpt("commentOut", ref mCommentOut, Predicates.IsTrue);
		}
		#endregion

		// really only implemented so we can search triggers in the Model's FindNameIndex
		#region IMegaloScriptAccessibleObject Members
		string IMegaloScriptAccessibleObject.CodeName {
			get { return base.Name; }
			set { base.Name = value;
				NotifyPropertyChanged(kCodeNameChanged);
		} }
		#endregion
	};
}
