
namespace KSoft.Blam.Megalo.Model
{
	using Variants = RuntimeData.Variants;

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed partial class MegaloScriptModelDecompilerState
	{
		public MegaloScriptModel Model { get; private set; }

		Proto.MegaloScriptDatabase Database { get { return Model.Database; } }
		Variants.GameEngineMegaloVariant Variant { get { return Model.MegaloVariant; } }

		public MegaloScriptModelDecompilerState(MegaloScriptModel model)
		{
			Model = model;
		}

		#region DecompileVariant
		void DecompileVariantStringTable()
		{
			var string_table = Variant.StringTable;

			for (int x = 0; x < string_table.Count; x++)
			{
				var sref = string_table[x];
				if (sref.JustEnglish && !string.IsNullOrWhiteSpace(sref.English))
					sref.CodeName = sref.English;
				else
					sref.CodeName = "String" + x.ToString(Util.InvariantCultureInfo);
			}

			if (Variant.BaseNameStringIndex.IsNotNone())
				string_table[Variant.BaseNameStringIndex].CodeName = "BaseName";
		}
		void DecompileVariantPlayerTraits()
		{
			var string_table = Variant.StringTable;

			foreach (var obj in Variant.PlayerTraits)
				if (obj.NameStringIndex.IsNotNone())
					obj.CodeName = string_table[obj.NameStringIndex].English;
		}
		void DecompileVariantUserDefinedOptions()
		{
			var string_table = Variant.StringTable;

			for (int x = 0; x < Variant.UserDefinedOptions.Count; x++)
			{
				var obj = Variant.UserDefinedOptions[x];
				if (obj.NameStringIndex.IsNotNone() &&
					!string.IsNullOrWhiteSpace(string_table[obj.NameStringIndex].English))
					obj.CodeName = string_table[obj.NameStringIndex].English;
				else
					obj.CodeName = "Option" + x.ToString(Util.InvariantCultureInfo);
			}
		}
		public void DecompileVariant()
		{
			DecompileVariantStringTable();
			DecompileVariantPlayerTraits();
			DecompileVariantUserDefinedOptions();
		}
		#endregion

		public void DecompileTriggers()
		{
			DecompileScriptTriggers();
			DecompileVirtualTriggers();
		}

		public void FixupUnionGroups()
		{
			foreach (var group in Model.UnionGroups)
				group.DecompilePostprocess();
		}

		public void DecompileHudWidgets()
		{
			for (int x = 0; x < Model.HudWidgets.Count; x++)
				Model.HudWidgets[x].CodeName = "Widget" + x;
		}

		public void DecompileGameStatistics()
		{
			var string_table = Variant.StringTable;

			foreach (var obj in Model.GameStatistics)
				if (obj.NameStringIndex.IsNotNone())
					obj.CodeName = string_table[obj.NameStringIndex].English;
		}

		#region DecompileObjectFilters
		void DecompileObjectFilterName(MegaloScriptObjectFilter filter, int filterIndex)
		{
			var sb = new System.Text.StringBuilder();

			sb.Append("ObjectFilter");

			if (!filter.HasParameters)
			{
				sb.Append(filterIndex);
			}
			else
			{
				bool prefix_underscore = true;

				if (filter.HasObjectTypeIndex)
				{
					string obj_type = Model.ToIndexName(Proto.MegaloScriptValueIndexTarget.ObjectType, filter.ObjectTypeIndex);

					if (prefix_underscore)
						sb.Append('_');
					sb.Append(obj_type);
					prefix_underscore = true;
				}
				if (filter.HasTeam)
				{
					string team_name = Proto.MegaloScriptEnum.ToMemberName(Database,
						Database.TeamDesignatorValueType, filter.Team);

					if (prefix_underscore)
						sb.Append('_');
					sb.Append(team_name);
					prefix_underscore = true;
				}
				if (filter.HasNumeric)
				{
					if (prefix_underscore)
						sb.Append('_');
					sb.Append(filter.Numeric.ToString(Util.InvariantCultureInfo));
					prefix_underscore = true;
				}
			}

			filter.CodeName = sb.ToString();
		}
		public void DecompileObjectFilters()
		{
			var string_table = Variant.StringTable;

			for (int x = 0; x < Model.ObjectFilters.Count; x++)
			{
				var obj = Model.ObjectFilters[x];
				if (obj.LabelStringIndex.IsNotNone())
					obj.CodeName = string_table[obj.LabelStringIndex].English;
				else
					DecompileObjectFilterName(obj, x);
			}
		}
		#endregion

		public void DecompileCandySpawnerFilters()
		{
			var string_table = Variant.StringTable;

			for (int x = 0; x < Model.CandySpawnerFilters.Count; x++)
			{
				var obj = Model.CandySpawnerFilters[x];
				if (obj.LabelStringIndex.IsNotNone())
					obj.CodeName = string_table[obj.LabelStringIndex].English;
				else
					obj.CodeName = "GameObjectFilter" + x.ToString(Util.InvariantCultureInfo);
			}
		}

		#region DecompileVariables
		void DecompileVariableNames(MegaloScriptModelVariableSet set)
		{
			string prefix = set.SetType != MegaloScriptVariableSet.Globals ? set.SetType.ToString() : "Global";
			var custom_var_proto_type = Database.VariableRefTypes[MegaloScriptVariableReferenceType.Custom];
			var sb = new System.Text.StringBuilder();

			for (int x = 0; x < set.Numerics.Count; x++, sb.Clear())
			{
				var variable = set.Numerics[x];
				var custom_var_proto_member = custom_var_proto_type.Members[variable.Var.Type];
				var value_type = custom_var_proto_member.ValueType;

				sb.Append(prefix);
				sb.Append("Numeric");

				if (value_type.BaseType == Proto.MegaloScriptValueBaseType.Index &&
					value_type.IndexTarget == Proto.MegaloScriptValueIndexTarget.Option)
				{
					var option = Variant.UserDefinedOptions[variable.Var.Data];
					sb.Append(option.CodeName.Replace(" ", ""));
				}
				else
					sb.Append(x);

				variable.CodeName = sb.ToString();
			}
			for (int x = 0; x < set.Timers.Count; x++, sb.Clear())
			{
				var variable = set.Timers[x];
				var custom_var_proto_member = custom_var_proto_type.Members[variable.Var.Type];
				var value_type = custom_var_proto_member.ValueType;

				sb.Append(prefix);
				sb.Append("Timer");

				if (value_type.BaseType == Proto.MegaloScriptValueBaseType.Index &&
					value_type.IndexTarget == Proto.MegaloScriptValueIndexTarget.Option)
				{
					var option = Variant.UserDefinedOptions[variable.Var.Data];
					sb.Append(option.CodeName.Replace(" ", ""));
				}
				else
					sb.Append(x);

				variable.CodeName = sb.ToString();
			}
			for (int x = 0; x < set.Teams.Count; x++, sb.Clear())
			{
				var variable = set.Teams[x];
				variable.CodeName = string.Format(Util.InvariantCultureInfo,
					"{0}{1}{2}", prefix, "Team", x.ToString(Util.InvariantCultureInfo));
			}
			for (int x = 0; x < set.Players.Count; x++, sb.Clear())
			{
				var variable = set.Players[x];
				variable.CodeName = string.Format(Util.InvariantCultureInfo,
					"{0}{1}{2}", prefix, "Player", x.ToString(Util.InvariantCultureInfo));
			}
			for (int x = 0; x < set.Objects.Count; x++, sb.Clear())
			{
				var variable = set.Objects[x];
				variable.CodeName = string.Format(Util.InvariantCultureInfo,
					"{0}{1}{2}", prefix, "Object", x.ToString(Util.InvariantCultureInfo));
			}
		}
		public void DecompileVariables()
		{
			DecompileVariableNames(Model.GlobalVariables);
			DecompileVariableNames(Model.PlayerVariables);
			DecompileVariableNames(Model.ObjectVariables);
			DecompileVariableNames(Model.TeamVariables);
		}
		#endregion
	};
}
