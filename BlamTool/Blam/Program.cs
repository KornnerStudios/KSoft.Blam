using System.Collections.Generic;
using Mono.Options;

namespace KSoft.Tool
{
	sealed class ProgramBlam : ProgramBase
	{
		protected override Environment ProgramEnvironment => Environment.Blam;
		public static void MainEntryPoint(List<string> args)
		{
			KSoft.Blam.Program.Initialize();
			KSoft.Blam.Program.InitializeCoreSystems();

			var prog = new ProgramBlam();
			prog.MainImpl(args);

			KSoft.Blam.Program.DisposeCoreSystems();
			KSoft.Blam.Program.Dispose();
		}

		#region ToolType
		[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
		enum ToolType
		{
			None,

			Gvar,
			Metadata,
		};
		static string GetValidTools()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid tools: ");

			sb.Append(ToolType.Gvar.ToString().ToLowerInvariant()).Append(',');
			sb.Append(ToolType.Metadata.ToString().ToLowerInvariant()).Append(',');

			return sb.ToString();
		}
		#endregion

		ToolType mToolType;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{ "tool=", GetValidTools(),
					v => Program.ParseEnum(v, out mToolType) },
			};
			InitializeOptionArgShowHelp();
		}

		void MainBody(List<string> args)
		{
			string help_name = "tool=" + mToolType.ToString().ToLowerInvariant();

			switch (mToolType)
			{
				case ToolType.Gvar: Blam.GameVariantTool.MainEntryPoint(help_name, args); break;
				case ToolType.Metadata: Blam.ContentMiniMetadataTool.MainEntryPoint(help_name, args); break;

				default: Program.UnavailableOption(mToolType); break;
			}
		}
		void MainImpl(List<string> args)
		{
			MainImpl_Prologue(args, out List<string> extra, () => mToolType == ToolType.None);
			MainImpl_Program(extra, MainBody);
		}
	};
}
