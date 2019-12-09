using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

using KBlam = KSoft.Blam;

namespace KSoft.Tool.Blam
{
	class ContentMiniMetadataTool : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.Blam; } }
		public static void _Main(string helpName, List<string> args)
		{
			var prog = new ContentMiniMetadataTool();
			prog.MainImpl(helpName, args);
		}

		[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
		enum Mode
		{
			None,

			Decode,
			Encode,
		};
		static string GetValidModes()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid modes: ");

			sb.AppendFormat("{0},", Mode.Decode.ToString().ToLowerInvariant());
			sb.AppendFormat("{0},", Mode.Encode.ToString().ToLowerInvariant());

			return sb.ToString();
		}

		const string kNameExtension = ".metadata";

		KBlam.Engine.EngineBuildHandle mGameBranchHandle;
		Mode mMode;
		string mMiniMetadata;
		string mOutputPath;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{"mode=", GetValidModes(),
					v => Program.ParseEnum(v, out mMode) },
				{"game=", "Game environment to operate in (HaloReach, Halo4, Halo2A)",
					v => mGameBranchHandle = KBlam.Engine.EngineRegistry.TryParseEngineBranchName(v) },
				{"name=", "Content mini metadata string",
					v => mMiniMetadata = v },
				{"out:", "Directory to output variant metadata (xml) files. Defaults to the source directory if blank",
					v => mOutputPath = v },
			};
			InitializeOptionArgShowHelp();
		}

		#region ValidateArgs
		bool ValidateArgsDecode()
		{
			if (!KBlam.RuntimeData.ContentMiniMetadata.ContainerNameIsValid(mMiniMetadata))
			{
				Console.WriteLine("Error: Mini-metadata provided isn't valid");
				return false;
			}

			if (string.IsNullOrWhiteSpace(mOutputPath)) mOutputPath = Path.Combine(System.Environment.CurrentDirectory, @"\");
			if (!Directory.Exists(mOutputPath))
			{
				Console.WriteLine("Error: The output path doesn't exist or is inaccessible: {0}", mOutputPath);
				return false;
			}

			return true;
		}
		bool ValidateArgsEncode()
		{
			if (string.IsNullOrWhiteSpace(mMiniMetadata))
			{
				Console.WriteLine("Error: Invalid name");
				return false;
			}

			if (string.IsNullOrWhiteSpace(mOutputPath)) mOutputPath = Path.Combine(System.Environment.CurrentDirectory, @"\");
			if (!Directory.Exists(mOutputPath))
			{
				Console.WriteLine("Error: The input path doesn't exist or is inaccessible: {0}", mOutputPath);
				return false;
			}

			return true;
		}

		protected override bool ValidateArgs()
		{
			if (mMode != Mode.None && mGameBranchHandle.IsNone)
			{
				Console.WriteLine("Error: Invalid game environment");
				return false;
			}

			switch (mMode)
			{
				case Mode.Decode: return ValidateArgsDecode();
				case Mode.Encode: return ValidateArgsEncode();

				default: return true;
			}
		}
		#endregion

		void MainBody()
		{
			switch (mMode)
			{
				case Mode.Decode: Decode(); break;
				case Mode.Encode: Encode(); break;

				default: Program.UnavailableOption(mMode); break;
			}
		}
		void MainImpl(string helpName, List<string> args)
		{
			List<string> extra;
			MainImpl_Prologue(args, out extra, () => mMode == Mode.None);
			MainImpl_Tool(helpName, "content mini-metadata", MainBody);
		}

		void Decode()
		{
			var metadata = KBlam.RuntimeData.ContentMiniMetadata.Decode(mGameBranchHandle, mMiniMetadata);

			string xml_filename = Path.Combine(mOutputPath, mMiniMetadata) + kNameExtension;

			using (var xml = IO.XmlElementStream.CreateForWrite("ContentMiniMetadata"))
			{
				xml.StreamMode = FileAccess.Write;

				metadata.Serialize(xml);

				using (var sw = new System.IO.StreamWriter(xml_filename, false, System.Text.Encoding.UTF8))
					xml.Document.Save(sw);
			}
		}
		void Encode()
		{
			var metadata = KBlam.RuntimeData.ContentMiniMetadata.Create(mGameBranchHandle);

			string xml_filename = Path.Combine(mOutputPath, mMiniMetadata) + kNameExtension;

			if (!File.Exists(xml_filename))
			{
				Console.WriteLine("Error: metadata xml does not exist in source directory {0}", xml_filename);
				return;
			}

			using (var xml = new IO.XmlElementStream(xml_filename))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Read;

				metadata.Serialize(xml);
			}

			if (metadata.BuildHandle != mGameBranchHandle)
			{
				Console.WriteLine("Error: {0}'s game parameter '{1}' differs from what you gave me. Weak sauce",
					xml_filename, metadata.BuildHandle);
				return;
			}

			Console.WriteLine("New mini metadata:");
			Console.WriteLine(metadata.Encode());
		}
	};
}