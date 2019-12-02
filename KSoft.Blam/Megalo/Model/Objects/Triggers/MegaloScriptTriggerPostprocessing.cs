using System.Collections.Generic;
using System.Linq;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModelCompilerState
	{
		sealed class TriggerCompiler
			: MegaloScriptTriggerProcessor
		{
			MegaloScriptModelCompilerState mOwnerState;
			int mNextRemapIndex;
			bool mCompileReferences;

			public TriggerCompiler(MegaloScriptModelCompilerState state) : base(state.Model, null)
			{
				base.TriggersToProcess =
					(from id in Model.TriggerExecutionOrder
						select Model.Triggers[id]).Union
					(from t in Model.Triggers
						where t.TriggerType != MegaloScriptTriggerType.Subroutine
						select t);

				mOwnerState = state;
				mCompileReferences = true;
			}

			protected override bool PreProcessTrigger(MegaloScriptTrigger root, MegaloScriptTrigger parent, MegaloScriptTriggerBase current)
			{
				mCompileReferences = true;

				var trigger = current as MegaloScriptTrigger;
				if (trigger != null)
				{
					if (!trigger.CommentOut)
					{
						mOwnerState.TriggerWriteOrder.Add(current.Id);
						mOwnerState.TriggerRemappings.Add(current.Id, mNextRemapIndex);
						mNextRemapIndex++;
					}
					else
						mCompileReferences = false;
				}

				return mCompileReferences;
			}
			protected override void PostProcessTrigger(MegaloScriptTrigger root, MegaloScriptTrigger parent, MegaloScriptTriggerBase current)
			{
				if (mCompileReferences)
					mOwnerState.Compile(current.References);
			}
		};

		void RemapTriggerIndex(ref int triggerIndex)
		{
			if (TriggerRemappings.Count > 0)
			{
				int remapped_index;
				bool remapped = TriggerRemappings.TryGetValue(triggerIndex, out remapped_index);
				Contract.Assert(remapped, "Failed to remap a trigger index");
				triggerIndex = remapped_index;
			}
		}
	};

	partial class MegaloScriptModelDecompilerState
	{
		sealed class TriggerSubroutineDecompiler
			: MegaloScriptTriggerProcessor
		{
			public TriggerSubroutineDecompiler(MegaloScriptModel model,
				IEnumerable<MegaloScriptTrigger> rootTriggers) : base(model, rootTriggers)
			{
			}

			// When processing (decompiling) a Trigger, current is a MegaloScriptTrigger
			// When processing (decompiling) a VirtualTrigger, current will be a MegaloScriptVirtualTrigger
			protected override bool PreProcessTrigger(MegaloScriptTrigger root, MegaloScriptTrigger parent, MegaloScriptTriggerBase current)
			{
				if (current.ObjectType != MegaloScriptModelObjectType.VirtualTrigger && string.IsNullOrEmpty(current.Name))
					current.Name = string.Format("{0}_Subroutine{1}", parent.Name, current.Id.ToString());

				return true;
			}
		};

		void DecompileScriptTriggers()
		{
			var root_triggers = new List<MegaloScriptTrigger>(Model.Triggers.Count);
			foreach (var obj in Model.Triggers)
			{
				if (obj.TriggerType != MegaloScriptTriggerType.Subroutine)
				{
					string prefix = obj.TriggerType != MegaloScriptTriggerType.Normal ? obj.TriggerType.ToString() : "";
					obj.Name = prefix + "Trigger";
					// if the trigger isn't an entry point
					if (obj.TriggerType == MegaloScriptTriggerType.Normal)
						obj.Name += obj.Id.ToString();

					root_triggers.Add(obj);
				}

				Decompile(obj.References);
			}
			if (Model.DoubleHostMigrationTriggerIndex.IsNotNone())
				Model.Triggers[Model.DoubleHostMigrationTriggerIndex].Name =
					Proto.MegaloScriptTriggerEntryPoints.DoubleHostMigration.ToString(); // Why? because find references will pick it up, that's why

			var subroutine_decompiler = new TriggerSubroutineDecompiler(Model, root_triggers);
			subroutine_decompiler.Process();
		}
		void DecompileVirtualTriggers()
		{
			foreach (var obj in Model.VirtualTriggers)
				Decompile(obj.References);
		}
	};
}