using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		protected abstract MegaloScriptTrigger NewTrigger();
		protected virtual MegaloScriptVirtualTrigger NewVirtualTrigger()
		{
			if (!Database.Limits.SupportsVirtualTriggers)
				throw new InvalidOperationException("Tried to create a virtual trigger in an Engine which doesn't support them");

			return new MegaloScriptVirtualTrigger();
		}

		#region CreateTrigger
		public MegaloScriptTrigger CreateTrigger(int id = TypeExtensions.kNone)
		{
			return CreateObjectPostprocess(Triggers, NewTrigger(), id);
		}
		public MegaloScriptVirtualTrigger CreateVirtualTrigger(int id = TypeExtensions.kNone)
		{
			return CreateObjectPostprocess(VirtualTriggers, NewVirtualTrigger(), id);
		}

		public MegaloScriptModelObjectHandle CreateForEachAction(MegaloScriptModelObjectHandle triggerHandle)
		{
			var activate_action = CreateAction();
			activate_action.InitializeForType(this, Database.ForEachAction.DBID);

			var action_arg0 = CreateValue(Database.GetValueType("TriggerReference"))
				as Megalo.Model.MegaloScriptIndexValue;
			action_arg0.Value = triggerHandle.Id;

			activate_action.Arguments.InitializeValues(action_arg0);

			return activate_action.Handle;
		}
		public MegaloScriptModelObjectHandle CreateBeginAction(MegaloScriptModelObjectHandle virtualTriggerHandle)
		{
			var vt_action = CreateAction();
			vt_action.InitializeForType(this, Database.BeginAction.DBID);

			var vt_action_arg0 = CreateValue(Database.GetValueType("VirtualTrigger"))
				as Megalo.Model.MegaloScriptVirtualTriggerValue;
			vt_action_arg0.VirtualTriggerHandle = virtualTriggerHandle;

			vt_action.Arguments.InitializeValues(vt_action_arg0);

			return vt_action.Handle;
		}
		#endregion

		internal void TriggerSwapLogicUnits(MegaloScriptModelObjectHandle triggerId,
			MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{
			Contract.Requires(triggerId.Type == MegaloScriptModelObjectType.Trigger || triggerId.Type == MegaloScriptModelObjectType.VirtualTrigger);
			Contract.Requires(triggerId.IsNotNone);

			var trigger = (MegaloScriptTriggerBase)this[triggerId];
			Contract.Assert(trigger != null);
			ReferencesSwapLogicUnits(trigger.References, lhs, rhs);
		}

		#region NewFrom* Utils
		static readonly Func<IO.BitStream, MegaloScriptModel, MegaloScriptTrigger> NewTriggerFromBitStream =
			(s, model) => model.CreateTrigger();
		static MegaloScriptTrigger NewTriggerFromTagStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model)
			where TDoc : class
			where TCursor : class
		{
			var result = model.NewTrigger();
			if (model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds())
			{
				result.Id = model.Triggers.FirstInactiveIndex; // #NOTE_BLAM: kind of hacky, but works
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		static readonly Func<IO.BitStream, MegaloScriptModel, MegaloScriptVirtualTrigger> NewVirtualTriggerFromBitStream =
			(s, model) => model.CreateVirtualTrigger();
		static MegaloScriptVirtualTrigger NewVirtualTriggerFromTagStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model)
			where TDoc : class
			where TCursor : class
			{ return model.NewVirtualTrigger(); }
		#endregion

		#region IBitStreamSerializable Members
		void StreamEntryPointTriggerIndex(IO.BitStream s, ref int triggerIndex, Proto.MegaloScriptTriggerEntryPoints entryPoint)
		{
			if (Database.Limits.Supports(entryPoint))
			{
				int local_trigger_index = triggerIndex;
				if (s.IsWriting)
					mCompilerState.RemapTriggerPointer(ref local_trigger_index);

				if (!Database.Limits.StreamEntryPointIndexAsPointer)
					s.StreamIndex(ref local_trigger_index, Database.Limits.Triggers.IndexBitLength); // pointer-has-value
				else
					s.StreamNoneable(ref local_trigger_index, Database.Limits.Triggers.CountBitLength); // pointer

				if (s.IsReading)
					triggerIndex = local_trigger_index;
			}
			else
				triggerIndex = -1;
		}
		protected void SerializeTriggerEntryPoints(IO.BitStream s)
		{
			StreamEntryPointTriggerIndex(s, ref		mInitializationTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.Initialization);
			StreamEntryPointTriggerIndex(s, ref		mLocalInitializationTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.LocalInitialization);
			StreamEntryPointTriggerIndex(s, ref		mHostMigrationTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.HostMigration);
			StreamEntryPointTriggerIndex(s, ref		mDoubleHostMigrationTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.DoubleHostMigration);
			StreamEntryPointTriggerIndex(s, ref		mObjectDeathEventTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.ObjectDeathEvent);
			StreamEntryPointTriggerIndex(s, ref		mLocalTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.Local);
			StreamEntryPointTriggerIndex(s, ref		mPregameTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.Pregame);
			StreamEntryPointTriggerIndex(s, ref		mIncidentTriggerIndex,
				Proto.MegaloScriptTriggerEntryPoints.Incident);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloScriptTriggerBase
		: MegaloScriptModelNamedObject
	{
		public MegaloScriptConditionActionReferences References { get; private set; }

		protected MegaloScriptTriggerBase()
		{
			References = new MegaloScriptConditionActionReferences();
		}
		protected MegaloScriptTriggerBase(MegaloScriptConditionActionReferences refs)
		{
			References = refs;
		}
	};
}
