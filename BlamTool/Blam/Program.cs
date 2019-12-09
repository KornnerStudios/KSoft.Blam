using System.Collections.Generic;
using Mono.Options;

namespace KSoft.Tool
{
	class ProgramBlam : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.Blam; } }
		public static void _Main(List<string> args)
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

			sb.AppendFormat("{0},", ToolType.Gvar.ToString().ToLowerInvariant());
			sb.AppendFormat("{0},", ToolType.Metadata.ToString().ToLowerInvariant());

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
				case ToolType.Gvar: Blam.GameVariantTool._Main(help_name, args); break;
				case ToolType.Metadata: Blam.ContentMiniMetadataTool._Main(help_name, args); break;

				default: Program.UnavailableOption(mToolType); break;
			}
		}
		void MainImpl(List<string> args)
		{
			List<string> extra;
			MainImpl_Prologue(args, out extra, () => mToolType == ToolType.None);
			MainImpl_Program(extra, MainBody);
		}
	};
}