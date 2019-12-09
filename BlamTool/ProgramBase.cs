#define CATCH_EXCEPTIONS

using System;
using System.Collections.Generic;
using Mono.Options;

namespace KSoft.Tool
{
	abstract class ProgramBase
	{
		protected abstract Environment ProgramEnvironment { get; }

		protected OptionSet mOptions;
		protected bool mArgShowHelp;

		protected void InitializeOptionArgShowHelp(bool useQuestionMark = false)
		{
			mOptions.Add(useQuestionMark ? "?" : "help",
				"show this message and exit",
				v => mArgShowHelp = v != null);
		}
		protected abstract void InitializeOptions();
		protected virtual bool ValidateArgs() { return true; }
		protected virtual void PostprocessParsedOptions() { }

		protected ProgramBase()
		{
			InitializeOptions();
		}

		protected static bool DependentAssemblyExists(string assemblyName, bool isFault = true)
		{
			assemblyName += ".dll";

			string asm_path_cwdir = System.IO.Path.Combine(System.Environment.CurrentDirectory, assemblyName);

			if (!System.IO.File.Exists(asm_path_cwdir))
			{
				Console.WriteLine("{0} was not found with this .exe! {1}",
					assemblyName,
					isFault ? "I need this DLL in order to run!" : "I'm probably going to crash soon...");
				return false;
			}
			return true;
		}

		protected static bool SwitchIsOn(string switches, int index, string switchCtxt, string switchDesc)
		{
			bool is_on = switches.Length >= (index+1) && switches[index] == '1';

			if(is_on && switchCtxt != null)
				Console.WriteLine("{0}: Switch enabled - {1}", switchCtxt, switchDesc);

			return is_on;
		}

		static bool NoShowHelpOverride() { return false; }
		/// <summary></summary>
		/// <param name="args"></param>
		/// <param name="extra"></param>
		/// <param name="env"></param>
		/// <param name="showHelpOverride">If true, we'll show the help even if parsing is successful</param>
		protected void TryParseOptions(List<string> args, out List<string> extra, Environment env,
			Func<bool> showHelpOverride)
		{
			if (!Program.TryParse(env, mOptions, args, out extra) || showHelpOverride())
				mArgShowHelp = true;
		}
		protected void TryParseOptions(List<string> args, out List<string> extra, Environment env)
		{
			TryParseOptions(args, out extra, env, NoShowHelpOverride);
		}
		protected bool ShowHelp(Environment env, string helpName = "")
		{
			bool help_shown = false;

			if (help_shown = mArgShowHelp)
				Program.ShowHelp(env, mOptions, helpName);

			return help_shown;
		}

		protected void MainImpl_Prologue(List<string> args, out List<string> extra,
			Func<bool> modeIsNone)
		{
			bool parsedOptionsSuccess = Program.TryParse(ProgramEnvironment, mOptions, args, out extra);

			if (!parsedOptionsSuccess || modeIsNone())
				mArgShowHelp = true;

			if (parsedOptionsSuccess)
			{
				PostprocessParsedOptions();
			}
		}
		protected void MainImpl_Program(List<string> extra, Action<List<string>> body)
		{
			if (mArgShowHelp || !ValidateArgs())
				Program.ShowHelp(ProgramEnvironment, mOptions);
			else
				body(extra);
		}
		protected void MainImpl_Tool(string helpName, string bodyName, Action body)
		{
			if (mArgShowHelp || !ValidateArgs())
				Program.ShowHelp(ProgramEnvironment, mOptions, helpName);
			else
			{
#if CATCH_EXCEPTIONS
				try
#endif
				{
					body();
				}
#if CATCH_EXCEPTIONS
				catch (Exception e)
				{
					Console.WriteLine("Exception while {0} processing: ", bodyName);
					Console.WriteLine(e.Message);
					Console.WriteLine();
					Console.WriteLine(e.StackTrace);
					if (e.InnerException != null)
					{
						Console.WriteLine();
						Console.WriteLine("InnerException: {0}", e.InnerException.Message);
					}
					if (System.Diagnostics.Debugger.IsAttached)
						throw;
				}
#endif
			}
		}
	};
}