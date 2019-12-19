using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Model
{
	using Variants = RuntimeData.Variants;

	[System.Reflection.Obfuscation(Exclude=false)]
	abstract class MegaloScriptTriggerProcessor
	{
		public MegaloScriptModel Model { get; private set; }

		protected Proto.MegaloScriptDatabase Database { get { return Model.Database; } }
		protected Variants.GameEngineMegaloVariant Variant { get { return Model.MegaloVariant; } }

		protected IEnumerable<MegaloScriptTrigger> TriggersToProcess { get; set; }

		protected MegaloScriptTriggerProcessor(MegaloScriptModel model,
			IEnumerable<MegaloScriptTrigger> triggersToProcess = null)
		{
			Model = model;
			TriggersToProcess = triggersToProcess ?? Model.Triggers;
		}

		// When processing a Trigger, current is a MegaloScriptTrigger
		// When processing a VirtualTrigger, current will be a MegaloScriptVirtualTrigger
		protected virtual bool PreProcessTrigger(MegaloScriptTrigger root, MegaloScriptTrigger parent, MegaloScriptTriggerBase current)
		{ return true; }
		protected virtual void PostProcessTrigger(MegaloScriptTrigger root, MegaloScriptTrigger parent, MegaloScriptTriggerBase current)
		{}
		void ProcessTriggerRecursive(MegaloScriptTrigger root, MegaloScriptTrigger parent, MegaloScriptTriggerBase current)
		{
			bool preprocessed = PreProcessTrigger(root, parent, current);

			if(preprocessed) foreach (var handle in current.References)
				if (handle.Type == MegaloScriptModelObjectType.Action)
				{
					var action = Model.Actions[handle.Id];

					if (action.ProtoData == Database.ForEachAction)
					{
						var index_param = action.GetArgument<MegaloScriptIndexValue>(Model, 0);
						int trigger_index = index_param.Value;
						var recurse_parent = current as MegaloScriptTrigger ?? parent;
						ProcessTriggerRecursive(root, recurse_parent, Model.Triggers[trigger_index]);
					}
					else if (action.ProtoData == Database.BeginAction)
					{
						var index_param = action.GetArgument<MegaloScriptVirtualTriggerValue>(Model, 0);
						int trigger_index = index_param.VirtualTriggerHandle.Id;
						ProcessTriggerRecursive(root, parent, Model.VirtualTriggers[trigger_index]);
					}
				}

			PostProcessTrigger(root, parent, current);
		}
		public void Process()
		{
			foreach (var trigger in TriggersToProcess)
				ProcessTriggerRecursive(trigger, trigger, trigger);
		}
	};
}