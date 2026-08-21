using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	sealed partial class MegaloScriptModelCompilerState
	{
		public MegaloScriptModel Model { get; private set; }
		public List<int> ConditionWriteOrder { get; private set; }
		public List<int> ActionWriteOrder { get; private set; }
		public List<int> TriggerWriteOrder { get; private set; }
		// UnionGroup.Id -> local union group index
		public Dictionary<int, int> UnionGroupRemappings { get; private set; }
		Dictionary<int, int> TriggerRemappings { get; /*private*/ set; }

		Proto.MegaloScriptDatabase Database { get { return Model.Database; } }

		public MegaloScriptModelCompilerState(MegaloScriptModel model)
		{
			Model = model;
			ConditionWriteOrder = new List<int>(model.Conditions.Count);
			UnionGroupRemappings = new Dictionary<int, int>(model.Conditions.Count);
			ActionWriteOrder = new List<int>(model.Actions.Count);
			TriggerWriteOrder = new List<int>(model.Triggers.Count);
			TriggerRemappings = new Dictionary<int, int>(model.Triggers.Count);
		}

		public void CompileTriggers()
		{
			var trigger_compiler = new TriggerCompiler(this);
			trigger_compiler.Process();
		}

		#region RemapTriggerIndex
		public void RemapTriggerReference(ref int triggerIndex)
		{
			if (triggerIndex.IsNone())
			{
				throw new ArgumentNoneException(nameof(triggerIndex));
			}

			RemapTriggerIndex(ref triggerIndex);
		}
		public void RemapTriggerPointer(ref int triggerIndex)
		{
			if (!triggerIndex.IsNoneOrPositive())
			{
				throw new System.ArgumentOutOfRangeException(nameof(triggerIndex), triggerIndex,
					"Trigger pointer must be NONE or non-negative.");
			}

			if (triggerIndex.IsNotNone())
			{
				RemapTriggerIndex(ref triggerIndex);
			}
		}
		#endregion

		#region PopulateObjectTypeReferences
		void PopulateObjectTypeReferencesFromParams(MegaloScriptModelObjectWithParameters obj)
		{
			if (!obj.Arguments.ProtoData.ContainsObjectTypeParameter)
			{
				return;
			}

			foreach (var value_id in obj.Arguments)
			{
				var value = Model.Values[value_id];

				if (value.ValueType.BaseType == Proto.MegaloScriptValueBaseType.Index &&
					value.ValueType.IndexTarget == Proto.MegaloScriptValueIndexTarget.ObjectType)
				{
					int type_index = ((MegaloScriptIndexValue)value).Value;
					Model.ObjectTypeReferenceAdd(type_index);
				}
			}
		}
		public void PopulateObjectTypeReferences()
		{
			Model.ObjectTypeReferencesClear();

			foreach (var cond_id in ConditionWriteOrder)
			{
				PopulateObjectTypeReferencesFromParams(Model.Conditions[cond_id]);
			}

			foreach (var action_id in ActionWriteOrder)
			{
				PopulateObjectTypeReferencesFromParams(Model.Actions[action_id]);
			}
		}
		#endregion
	};
}