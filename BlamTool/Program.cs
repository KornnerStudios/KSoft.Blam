//#define TEST_ARGS
//#define TEST_ENV_BLAM_GVAR
#define CATCH_EXCEPTIONS

using System;
using System.Collections.Generic;
using Mono.Options;

/*
 * KSoft.Tool
 *	?
 *	env=
 *		blam
*/

namespace KSoft.Tool
{
	partial class Program : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.None; } }

		static string gName;
		public static string GetName()
		{
			if (gName == null)
				gName = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Name;

			return gName;
		}
		static string gVersion;
		public static string GetVersion()
		{
			if (gVersion == null)
				gVersion = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Version.ToString();

			return gVersion;
		}

		static void Initialize()
		{
			KSoft.Program.Initialize();
		}
		static void Dispose()
		{
			KSoft.Program.Dispose();
		}

		static void Main(string[] args)
		{
			Console.WriteLine(); // newline, because long command lines don't go great with real messages

			DependentAssemblyExists("ICSharpCode.SharpZipLib", false);
			if (!DependentAssemblyExists("KSoft"))
				return;
			if (!DependentAssemblyExists("KSoft.Blam"))
				return;

			Initialize();

			var prog = new KSoft.Tool.Program();
#if CATCH_EXCEPTIONS
			try
#endif
			{
				prog.MainImpl(args);
			}
#if CATCH_EXCEPTIONS
			catch (Exception e)
			{
				Console.Write("UNHANDLED EXCEPTION: ");
				Console.WriteLine(e);
				if (System.Diagnostics.Debugger.IsAttached)
					throw;
			}
#endif

			Dispose();

			if (System.Diagnostics.Debugger.IsAttached)
			{
				Console.WriteLine("Done");
				Console.Read();
			}
		}

		#region Environment
		static string GetValidEnvironments()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid environments: ");

			sb.AppendFormat("{0},", Environment.Blam.ToString().ToLowerInvariant());

			return sb.ToString(0, sb.Length-1); // don't include the last ','
		}
		static string ToString(Environment env)
		{
			if (env == Environment.None)
				return "Main";

			return env.ToString();
		}
		#endregion

		Environment mArgEnv;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{ "env=", GetValidEnvironments(),
					v => ParseEnum(v, out mArgEnv) },
			};
			InitializeOptionArgShowHelp(useQuestionMark:true);
		}

#if TEST_ARGS
		static string[] kTestArgs = {
	#if TEST_ENV_BLAM_GVAR
			"-env=blam",
				"-tool=gvar",
		#if !GVAR_DECODE
					"-mode=" +		 "decode",
					"-switches=" +	 "1",
		#else
					"-mode=" +		 "encode",
					"-switches=" +	 "01",
		#endif
					"-stopwatch",
					"-game=" +		 "HaloReach",
					//"-offset=" +	 "53248",
					//"-path=" +		@"C:\Mount\A\Bungie\Games\Halo4\Xbox\Retail\variants\Halomods\gfigmecdafz545555eo4szqgtcysgxrmv12ihzebo",
					//"-path=" +		@"C:\Mount\A\Bungie\Games\Halo4\Xbox\Retail\variants\Halomods\HalomodsCTF.variant",
					//"-path=" +		@"C:\Mount\A\Bungie\Games\Halo4\Xbox\Retail\variants\New\gfigmecdafz545555em4szqgtcysgxrmv12ihzebo.ctf_tu",
					//"-name=" +		 "CTF",
					//"-path=" +		@"C:\Mount\A\Bungie\Games\Halo4\Xbox\Retail\variants\New\campaign.game",
					//"-name=" +		 "campaign",
					"-path=" +		@"C:\Mount\A\Bungie\Games\HaloReach\Xbox\Retail\variants\Race\RaceTest.variant",
					"-name=" +		 "Race",
	#endif
		};
#endif
		void MainImpl(string[] args)
		{
#if TEST_ARGS
			args = kTestArgs;
#endif
			//return;

			List<string> extra;
			if (!TryParse(Environment.None, mOptions, args, out extra) || mArgEnv == Environment.None)
				mArgShowHelp = true;

			if (mArgShowHelp)
				ShowHelp(Environment.None, mOptions);
			else
			{
				switch (mArgEnv)
				{
					case Environment.Blam:
						ProgramBlam._Main(extra);
						break;

					default:
						UnavailableOption(mArgEnv);
						break;
				}
			}
		}

		#region Util
		public static bool TryParse(Environment env, OptionSet ops, IEnumerable<string> args, out List<string> extra)
		{
			bool result = true;
			extra = null;

			string help = env == Environment.None ? "?" : "help";

			try { extra = ops.Parse(args); }
			catch (OptionException e)
			{
				Console.Write("{0}:", GetName());
				Console.WriteLine(e.Message);
				Console.WriteLine("Use {0} for more information", help);
				result = false;
			}

			return result;
		}
		public static void ShowHelp(Environment env, OptionSet ops, string subtoolName = "")
		{
			Console.WriteLine();
			Console.WriteLine("Version: {0}", GetVersion());
			Console.WriteLine("Usage: {0} <environment> <environment-options>", GetName());
			Console.WriteLine("{0} Options: {1}", ToString(env), subtoolName);
			ops.WriteOptionDescriptions(Console.Out);
		}
		public static void UnavailableOption<T>(T option)
		{
			//string option_str = option == null ? null : option.ToString();

			Console.WriteLine("I don't think so scooter");
		}

		public static void ParseEnum<T>(string input, out T value)
			where T : struct
		{
			Enum.TryParse<T>(input, true, out value);
		}
		public static void ParseEnum<T>(string input, out T value, T invalidValue)
			where T : struct
		{
			if (!Enum.TryParse<T>(input, true, out value))
				value = invalidValue;
		}
		#endregion
	};
}
