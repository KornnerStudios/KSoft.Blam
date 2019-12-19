using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Options;

namespace KSoft.Tool.Blam
{
	using KBlam = KSoft.Blam;
	using MegaloModel = KSoft.Blam.Megalo.Model;
	using MegaloProto = KSoft.Blam.Megalo.Proto;

	class GameVariantTool : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.Blam; } }
		public static void _Main(string helpName, List<string> args)
		{
			var prog = new GameVariantTool();
			prog.MainImpl(helpName, args);
		}

		[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
		enum Mode
		{
			None,

			Decode,
			Encode,
			DecodeMass,
		};
		static string GetValidModes()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid modes: ");

			sb.AppendFormat("{0},", Mode.Decode.ToString().ToLowerInvariant());
			sb.AppendFormat("{0},", Mode.Encode.ToString().ToLowerInvariant());

			return sb.ToString();
		}

		const string kNameExtension = ".xml";
		const string kGvarExtension = ".bin";

		KBlam.Engine.BlamEngineTargetHandle mGameBuildAndTarget = KBlam.Engine.BlamEngineTargetHandle.None;
		KBlam.Engine.EngineSystemReference<KBlam.Blob.BlobSystem> mGameEngineBlobSystemRef = KBlam.Engine.EngineSystemReference<KBlam.Blob.BlobSystem>.None;
		KBlam.Engine.EngineSystemReference<KBlam.Localization.LanguageSystem> mGameEngineLangystemRef = KBlam.Engine.EngineSystemReference<KBlam.Localization.LanguageSystem>.None;
		Mode mMode;
		string mPath;
		uint mFileOffset;
		string mName, mOutputPath, mSwitches;
		int mEngineVersion;
		bool mTimeOperation;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{"mode=", GetValidModes(),
					v => Program.ParseEnum(v, out mMode) },
				{"game=", "Game environment to operate in (HaloReach, Halo4, Halo2A)",
					ParseGameBuildFromOptionValue },
				{"path=", "Source/target container/file",
					v => mPath = v },
				{"offset:", "Source/target file offset where the BLF stream begins. Defaults to zero if blank",
					(uint v) => mFileOffset = v },
				{"name=", "The name of the source/output variant (.xml/.bin) file without an extension",
					v => mName = v },
				{"out:", "Directory to output variant xml files. Defaults to the source directory if blank",
					v => mOutputPath = v },
				{"switches:", "Mode specific switches",
					v => mSwitches = v },
				{"version:", "Engine version to use for non-MP variants. Defaults to zero if blank",
					(int v) => mEngineVersion = v },
				{"stopwatch", "Time performance",
					v => mTimeOperation = v != null },
				{"ignoreInvalidHash", "Causes invalid data hases to be ignored",
					v => { if (v != null) KBlam.Blob.GameEngineVariantBlob.RequireValidHashes = false; } },
				{"debugScriptDatabase", "Print error messages about the megalo script db to console",
					v => { if (v != null) MegaloProto.MegaloProtoSystem.OutputMegaloDatabasePostprocessErrorTextToConsole = true; } },
			};
			InitializeOptionArgShowHelp();
		}

		protected override void PostprocessParsedOptions()
		{
			if (mEngineVersion == 0 && mGameBuildAndTarget.Build.RevisionIndex.IsNotNone())
			{
				mEngineVersion = mGameBuildAndTarget.Build.Revision.Version;
			}
		}

		void ParseGameBuildFromOptionValue(string v)
		{
			KBlam.Engine.EngineBuildRevision build_revision = KBlam.Engine.EngineRegistry.TryParseExportedBuildName(v);
			if (build_revision != null)
			{
				mGameBuildAndTarget = build_revision.BuildHandle.ToEngineTargetHandle();
				mGameEngineBlobSystemRef = KBlam.Engine.EngineRegistry.GetSystem<KBlam.Blob.BlobSystem>(mGameBuildAndTarget.Build);
				mGameEngineLangystemRef = KBlam.Engine.EngineRegistry.GetSystem<KBlam.Localization.LanguageSystem>(mGameBuildAndTarget.Build);
			}
		}

		#region ValidateArgs
		bool ValidatePathAndName()
		{
			if (string.IsNullOrWhiteSpace(mPath) || string.IsNullOrWhiteSpace(mName))
			{
				Console.WriteLine("Error: Invalid path or name");
				return false;
			}

			return true;
		}
		bool ValidateArgsDecode()
		{
			if (!ValidatePathAndName())
				return false;

			if (!File.Exists(mPath))
			{
				Console.WriteLine("Error: Input game file does not exist");
				return false;
			}

			return true;
		}
		bool ValidateArgsDecodeMass()
		{
			if (string.IsNullOrWhiteSpace(mPath))
			{
				Console.WriteLine("Error: Invalid path");
				return false;
			}

			if (!File.Exists(mPath))
			{
				Console.WriteLine("Error: Input game file does not exist");
				return false;
			}

			return true;
		}
		bool ValidateArgsEncode()
		{
			if (!ValidatePathAndName())
				return false;

			return true;
		}
		protected override bool ValidateArgs()
		{
			if (mMode != Mode.None && (mGameBuildAndTarget.IsNone || mGameEngineBlobSystemRef.IsNotValid))
			{
				Console.WriteLine("Error: Invalid game environment");
				return false;
			}

			switch (mMode)
			{
				case Mode.Decode: return ValidateArgsDecode();
				case Mode.Encode: return ValidateArgsEncode();
				case Mode.DecodeMass: return ValidateArgsDecodeMass();

				default: return true;
			}
		}
		#endregion

		void MainBody()
		{
			var stopwatch = mTimeOperation ? System.Diagnostics.Stopwatch.StartNew() : null;

#if false
			if (mGameBuildAndTarget.Build.IsWithinSameBranch(KBlam.Engine.EngineRegistry.EngineBranchHaloReach))
				mCategoryIndexToName = CategoryIndexToNameReach;
			if (mGameBuildAndTarget.Build.IsWithinSameBranch(KBlam.Engine.EngineRegistry.EngineBranchHalo4))
				mCategoryIndexToName = CategoryIndexToNameHalo4;
#endif

			var megalo_proto_system_ref = KBlam.Engine.EngineSystemReference<MegaloProto.MegaloProtoSystem>.None;
			#region load game databases
			try
			{
				megalo_proto_system_ref = KBlam.Engine.EngineRegistry.GetSystem<MegaloProto.MegaloProtoSystem>(mGameBuildAndTarget.Build);

				var megalo_proto_system = megalo_proto_system_ref.System;
				var all_dbs_tasks = megalo_proto_system.GetAllDatabasesAsync(mGameBuildAndTarget.Build);

				System.Threading.Tasks.Task.WaitAll
					( all_dbs_tasks.Item1
					, all_dbs_tasks.Item2
					);

				megalo_proto_system.PrepareDatabasesForUse(all_dbs_tasks.Item1.Result, all_dbs_tasks.Item2.Result);
			}
			catch (Exception e)
			{
				Console.Write("Exception while initializing: ");
				Console.WriteLine(e);
			}

			if (!megalo_proto_system_ref.IsValid)
			{
				Console.WriteLine("Error: Failed to initialize");
			}
			#endregion

			if (mTimeOperation)
			{
				stopwatch.Stop();
				Console.WriteLine("Perf (DB): {0}", stopwatch.Elapsed);
				stopwatch.Restart();
			}

			if (megalo_proto_system_ref.IsValid) switch (mMode)
			{
				case Mode.Decode: Decode(mPath, mName, mOutputPath); break;
				case Mode.Encode: Encode(mPath, mName, mOutputPath); break;

				default: Program.UnavailableOption(mMode); break;
			}

			megalo_proto_system_ref.Dispose();
			mGameEngineBlobSystemRef.Dispose();
			mGameEngineLangystemRef.Dispose();

			if (mTimeOperation)
			{
				stopwatch.Stop();
				Console.WriteLine("Perf: {0}", stopwatch.Elapsed);
			}
		}
		void MainImpl(string helpName, List<string> args)
		{
			List<string> extra;
			MainImpl_Prologue(args, out extra, () => mMode == Mode.None);
			MainImpl_Tool(helpName, "game variant", MainBody);
		}

		#region CategoryIndexToName
#if false
		const int kCommunityCategoryIndex = 25;
		const string kCommunityCategory = "Community";

		Dictionary<int, string> mCategoryIndexToName;
		static Dictionary<int, string> CategoryIndexToNameReach = new Dictionary<int, string>() {
			{-1, ""},
			{0, "Capture the Flag" },
			{1, "Slayer" },
			{2, "Oddball" },
			{3, "King of the Hill" },
			{4, "Juggernaut" },
			{5, "Territories" },
			{6, "Assault" },
			{7, "Infection" },
			{8, "VIP" },
			{9, "Invasion" },
			{10, "Stockpile" },
			//{11, "" },
			{12, "Race" },
			{13, "Headhunter" },
			//{14, "" },
			//{15, "" },
			{16, "Insane" },
			{24, "Halomods.com" },
			{kCommunityCategoryIndex, kCommunityCategory },
		};
		static Dictionary<int, string> CategoryIndexToNameHalo4 = new Dictionary<int, string>() {
			{-1, ""},
			{0, "Capture the Flag" },
			{1, "Slayer" },
			{2, "Oddball" },
			{3, "King of the Hill" },
			{4, "Juggernaut" },			// none
			{5, "Flood" },
			{6, "Race" },				// none
			{7, "Extraction" },
			{8, "Dominion" },
			{9, "Regicide" },
			{10, "Payback" },			// none
			{11, "Grifball" },
			//{12, "" },
			//{13, "" },
			//{14, "" },
			{15, "Ricochet" },
			{16, "Halomods.com" },
			{20, "Infection" },
			{kCommunityCategoryIndex, kCommunityCategory },
		};
#endif
		#endregion

		#region Decode
		void DecodeParseSwitches(string switches,
			out MegaloModel.MegaloScriptModelTagElementStreamFlags modelStreamFlags,
			ref bool ignoreWritePredicates)
		{
			modelStreamFlags =
				MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjects |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.UseEnumNames |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.UseIndexNames |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteConditionTypeNames |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteActionTypeNames |

				MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjectsWriteSansIds
				;
			if (switches == null) switches = "";
			const string k_switches_ctxt = "GameVariant:Decode";

			bool using_op_names = false;
			if (SwitchIsOn(switches, 0, k_switches_ctxt + ":Megalo", "Use operation names (instead of DBIDs)"))
			{
				EnumFlags.Add(ref modelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseConditionTypeNames |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseActionTypeNames);

				EnumFlags.Remove(ref modelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteConditionTypeNames |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteActionTypeNames);

				using_op_names = true;
			}
			if (SwitchIsOn(switches, 1, k_switches_ctxt + ":Megalo", "Write operation parameter names"))
			{
				EnumFlags.Add(ref modelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteParamNames);
			}
			if (SwitchIsOn(switches, 2, k_switches_ctxt + ":Megalo", "Write operation parameter contexts"))
			{
				EnumFlags.Add(ref modelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteParamKinds);
			}
			if (mGameBuildAndTarget.Build.IsWithinSameBranch(KBlam.Engine.EngineRegistry.EngineBranchHaloReach) &&
				SwitchIsOn(switches, 3, k_switches_ctxt + ":Megalo", "Try to port Reach operations to H4"))
			{
				if (using_op_names)
					Console.WriteLine("\tIgnoring switch since you have me writing operation names");
				else
					EnumFlags.Add(ref modelStreamFlags,
						MegaloModel.MegaloScriptModelTagElementStreamFlags.TryToPort);
			}
			if (SwitchIsOn(switches, 4, k_switches_ctxt + ":Megalo", "Don't use enum/index names"))
			{
				EnumFlags.Remove(ref modelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseEnumNames |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseIndexNames);
			}
			if (SwitchIsOn(switches, 5, k_switches_ctxt + ":Megalo", "Serialize with object IDs"))
			{
				EnumFlags.Remove(ref modelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjectsWriteSansIds);
			}
			if (SwitchIsOn(switches, 6, k_switches_ctxt + ":Megalo", "Don't embed objects"))
			{
				EnumFlags.Remove(ref modelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjects |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjectsWriteSansIds);
			}
			if (SwitchIsOn(switches, 7, k_switches_ctxt + ":Megalo", "Always write 'default' data"))
			{
				ignoreWritePredicates = true;
			}
		}
		bool DecodeVariantBlob(FileStream fs,
			out KBlam.RuntimeData.Variants.GameEngineVariant gev)
		{
			gev = null;
			KBlam.Blob.GameEngineVariantBlob gevb = null;
			var blf_result = KBlam.Blob.Transport.BlobChunkVerificationResultInfo.ValidResult;

			long blffile_length = KBlam.Blob.GameEngineVariantBlob.GetBlfFileLength(mGameBuildAndTarget);
			using (var blf = new KBlam.Blob.Transport.BlobTransportStream())
			{
				blf.GameTarget = mGameBuildAndTarget;

				var blob_system = mGameEngineBlobSystemRef.System;

				blf_result = blf.OpenRange(fs, mFileOffset, mFileOffset + blffile_length, FileAccess.Read);
				if (blf_result.IsValid)
				{
					blf.UnderlyingStream.StreamMode = FileAccess.Read;

					IEnumerable<KBlam.Blob.BlobObject> objects;
					blf_result = blf.EnumerateChunks(blob_system, out objects);

					if (blf_result.IsValid)
					{
						gevb = (from bo in objects
								where bo is KBlam.Blob.GameEngineVariantBlob
								select bo).FirstOrDefault() as KBlam.Blob.GameEngineVariantBlob;
					}
				}
			}

			if (blf_result.IsInvalid)
			{
				Console.WriteLine("Error: Failed to decode variant file{0}{1}", System.Environment.NewLine,
					blf_result.BuildErrorMessage());
			}
			else if (gevb == null)
			{
				Console.WriteLine("Error: Not a game variant file");
			}
			else if (KBlam.Blob.GameEngineVariantBlob.RequireValidHashes && gevb.InvalidData)
			{
				Console.WriteLine("Error: Game variant file's bitstream is corrupt");
			}
			else
			{
				gev = gevb.Data;
			}

			return blf_result.IsValid;
		}
		bool DecodeVariantBlf(string filePath, out KBlam.RuntimeData.Variants.GameEngineVariant gev)
		{
			gev = null;
			bool result = true;

			using (var dst_fs = File.Open(filePath, System.IO.FileMode.Open, FileAccess.Read))
			{
				long blffile_length = KBlam.Blob.GameEngineVariantBlob.GetBlfFileLength(mGameBuildAndTarget);

				if (dst_fs.Length < (mFileOffset + blffile_length))
				{
					Console.WriteLine("Error: not enough data starting at offset 0x{0} to be a variant we support",
						mFileOffset.ToString("X8"));
					result = false;
				}
				else
				{
					result = DecodeVariantBlob(dst_fs, out gev);
				}
			}

			return result;
		}
		void DecodeSaveVariant(KBlam.RuntimeData.Variants.GameEngineVariant gev, string xmlFilename,
			MegaloModel.MegaloScriptModelTagElementStreamFlags modelStreamFlags, bool ignoreWritePredicates)
		{
			var megalo_variant = gev.TryGetMegaloVariant();
			if (megalo_variant != null)
			{
				megalo_variant.TagElementStreamSerializeFlags =
					KBlam.RuntimeData.Variants.GameEngineMegaloVariantTagElementStreamFlags.UseStringTableNames |
					KBlam.RuntimeData.Variants.GameEngineMegaloVariantTagElementStreamFlags.UseUserOptionNames;
				megalo_variant.EngineDefinition.TagElementStreamSerializeFlags =
					modelStreamFlags;
			}

			using (var xml = IO.XmlElementStream.CreateForWrite("GameVariant"))
			{
				xml.StreamMode = FileAccess.Write;
				xml.IgnoreWritePredicates = ignoreWritePredicates;

				gev.Serialize(xml);

				using (var sw = new System.IO.StreamWriter(xmlFilename, false, System.Text.Encoding.UTF8))
					xml.Document.Save(sw);
			}
		}
		void Decode(string filePath, string xmlName, string outputPath)
		{
			if (string.IsNullOrWhiteSpace(outputPath)) outputPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(outputPath))
			{
				Console.WriteLine("Error: The output path doesn't exist or is inaccessible: {0}", outputPath);
				return;
			}

			string xml_filename = Path.Combine(outputPath, xmlName) + kNameExtension;
			string bin_filename = filePath;

			MegaloModel.MegaloScriptModelTagElementStreamFlags model_stream_flags;
			bool ignore_write_predicates = false;
			DecodeParseSwitches(mSwitches, out model_stream_flags, ref ignore_write_predicates);

			KBlam.RuntimeData.Variants.GameEngineVariant gev = null;
			if (DecodeVariantBlf(bin_filename, out gev))
			{
				DecodeSaveVariant(gev, xml_filename, model_stream_flags, ignore_write_predicates);
			}
		}
		#endregion

		#region Encode
		[Flags]
		enum EncodeSwitches
		{
			CreateVariantFile=1<<0,
			ClearWeaponTuning=1<<1,
			AddAllTeams=1<<2,
		};
		void EncodeParseSwitches(string switches,
			out EncodeSwitches flags)
		{
			flags = 0;
			if (switches == null) switches = "";
			const string k_switches_ctxt = "GameVariant:Encode";

			if (SwitchIsOn(switches, 0, k_switches_ctxt + ":Megalo", "Clear TU weapon tuning data"))
			{
				flags |= EncodeSwitches.ClearWeaponTuning;
			}
			if (SwitchIsOn(switches, 1, k_switches_ctxt, "Create output game file"))
			{
				flags |= EncodeSwitches.CreateVariantFile;
				mFileOffset = 0;
			}
			if (SwitchIsOn(switches, 2, k_switches_ctxt, "Add all 8 teams"))
			{
				flags |= EncodeSwitches.AddAllTeams;
			}
		}
		bool EncodeVariantPreprocess(EncodeSwitches switches,
			KBlam.RuntimeData.Variants.GameEngineVariant gev, string filePath)
		{
			const string k_teams_xml_filename = @"Games\Halo4\Definitions\VariantGameTeams.xml";

			var mv = gev.TryGetMegaloVariant();

			#region CreateVariantFile
			if ((switches & EncodeSwitches.CreateVariantFile) != 0)
			{
				string dir = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(dir))
				{
					Console.WriteLine("Error: The directory for the game file doesn't exist or is inaccessible: {0}",
						dir);
					return false;
				}

				using (var fs = File.Create(filePath))
				{
					fs.SetLength(KBlam.Blob.GameEngineVariantBlob.GetBlfFileLength(mGameBuildAndTarget));
				}
			}
			else if (!File.Exists(filePath))
			{
				Console.WriteLine("Error: Output game file does not exist");
				return false;
			}
			#endregion
			#region ClearWeaponTuning
			if ((switches & EncodeSwitches.ClearWeaponTuning) != 0 && mv != null)
			{
				mv.ClearWeaponTunings();
			}
			#endregion
			#region AddAllTeams
			if ((switches & EncodeSwitches.AddAllTeams) != 0 && mGameBuildAndTarget.Build.IsWithinSameBranch(KBlam.Engine.EngineRegistry.EngineBranchHalo4))
			{
				var bv = gev.Variant.BaseVariant;

				foreach (var team in bv.TeamOptions.Teams)
					team.NameString.Clear();

				if (File.Exists(k_teams_xml_filename))
					using (var xml = new IO.XmlElementStream(k_teams_xml_filename, System.IO.FileAccess.Read))
					{
						xml.InitializeAtRootElement();
						xml.StreamMode = System.IO.FileAccess.Read;

						var team_xml_game = KBlam.Engine.EngineBuildHandle.None;
						KBlam.Engine.EngineBuildHandle.Serialize(xml, ref team_xml_game);
						if (team_xml_game.IsWithinSameBranch(KBlam.Engine.EngineRegistry.EngineBranchHalo4))
							bv.TeamOptions.SerializeTeams(xml);
						else
							Console.WriteLine("Warning: Teams xml is not for this game, not modifying variant's team data");
					}
				else
					Console.WriteLine("Warning: Teams xml not found, not modifying variant's team data");
			}
			#endregion

			if (mv == null)
				return true;

			#region sanity check category name
#if false
			string megalo_category_name;
			if (mv != null && mCornbread==false && mCategoryIndexToName.TryGetValue(mv.EngineCategory, out megalo_category_name))
			{
				if (mv.CategoryString.Count == 0)
					mv.CategoryString.Add(megalo_category_name);
				else if (mv.CategoryString[0].English != megalo_category_name)
					mv.CategoryString[0].Set(megalo_category_name);
			}
			else if (mv != null && mCornbread==false)
			{
				Console.WriteLine("Warning: Invalid Megalo category data {0}, defaulting",
					mv.EngineCategory);

				mv.BaseVariant.Header.EngineCategoryIndex = mv.EngineCategory = kCommunityCategoryIndex;
				if (mv.CategoryString.Count == 0)
					mv.CategoryString.Add(kCommunityCategory);
				else
					mv.CategoryString[0].Set(kCommunityCategory);
			}
#endif
			#endregion
			#region watermark
#if false
			if (mv != null)
			{
				var mv_h4 = mv as KBlam.Games.Halo4.RuntimeData.Variants.GameEngineMegaloVariantHalo4;

				if (mv_h4 != null)
				{
					const string k_water_mark = "Halomods in your pods!";

					if (mv_h4.IntroDescriptionString.Count == 0)
						mv_h4.IntroDescriptionString.Add(k_water_mark);
					else
						mv_h4.IntroDescriptionString[0].Set(k_water_mark);
				}
			}
#endif
			#endregion

			return true;
		}
		bool EncodeLoadVariant(string xmlFilename, out KBlam.RuntimeData.Variants.GameEngineVariant ev)
		{
			ev = new KBlam.RuntimeData.Variants.GameEngineVariant(mGameBuildAndTarget.Build);
			bool result = true;

			using (var xml = new IO.XmlElementStream(xmlFilename))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Read;

				ev.Serialize(xml);
			}

			if (ev.GameBuild != mGameBuildAndTarget.Build)
			{
				Console.WriteLine("Error: {0}'s game parameter '{1}' differs from what you gave me. Weak sauce",
					xmlFilename, ev.GameBuild);
				result = false;
			}

			return result;
		}
		bool EncodeVariantBlob(FileStream fs,
			KBlam.RuntimeData.Variants.GameEngineVariant gev)
		{
			var megalo_variant = gev.TryGetMegaloVariant();
			var blf_result = KBlam.Blob.Transport.BlobChunkVerificationResultInfo.ValidResult;

			long blffile_length = KBlam.Blob.GameEngineVariantBlob.GetBlfFileLength(mGameBuildAndTarget);
			using (var blf = new KBlam.Blob.Transport.BlobTransportStream())
			{
				blf.GameTarget = mGameBuildAndTarget;

				var blob_system = mGameEngineBlobSystemRef.System;

				blf_result = blf.OpenForWrite(fs, mFileOffset, blffile_length);
				if (blf_result.IsValid)
				{
					blf.UnderlyingStream.StreamMode = FileAccess.Write;

					var chdr = (KBlam.Blob.ContentHeaderBlob)blob_system.CreateObject(mGameBuildAndTarget, KBlam.Blob.WellKnownBlob.ContentHeader);
					chdr.ChangeData(gev.Variant.BaseVariant.Header,
						megalo_variant != null ? megalo_variant.EngineVersion : mEngineVersion);

					var mpvr = (KBlam.Blob.GameEngineVariantBlob)blob_system.CreateObject(mGameBuildAndTarget, KBlam.Blob.WellKnownBlob.GameVariant);
					mpvr.ChangeData(gev);

					blf_result = blf.WriteChunksSansAuthentication(chdr, mpvr);
				}
			}

			if (blf_result.IsInvalid)
			{
				Console.WriteLine("Error: Failed to encode variant file{0}{1}", System.Environment.NewLine,
					blf_result.BuildErrorMessage());
			}

			return blf_result.IsValid;
		}
		bool EncodeVariantBlf(string filePath, KBlam.RuntimeData.Variants.GameEngineVariant gev)
		{
			bool result = true;

			using (var dst_fs = File.Open(filePath, System.IO.FileMode.Open, FileAccess.Write))
				result = EncodeVariantBlob(dst_fs, gev);

			return result;
		}
		void Encode(string filePath, string xmlName, string outputPath)
		{
			EncodeSwitches switches;
			EncodeParseSwitches(mSwitches, out switches);
			bool create_output_dir_and_file = (switches & EncodeSwitches.CreateVariantFile) != 0;

			if (string.IsNullOrWhiteSpace(outputPath)) outputPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(outputPath) && !create_output_dir_and_file)
			{
				Console.WriteLine("Error: The output path doesn't exist or is inaccessible: {0}", outputPath);
				return;
			}

			string xml_filename = filePath;
			string bin_filename = Path.Combine(outputPath, xmlName) + kGvarExtension;

			if (!File.Exists(xml_filename))
			{
				Console.WriteLine("Error: input variant xml does not exist in {0}", xml_filename);
				return;
			}

			bool success = true;
			KBlam.RuntimeData.Variants.GameEngineVariant gev = null;
			success = success && EncodeLoadVariant(xml_filename, out gev);
			success = success && EncodeVariantPreprocess(switches, gev, bin_filename);
			success = success && EncodeVariantBlf(bin_filename, gev);

			if (!success && create_output_dir_and_file &&
				File.Exists(bin_filename))
			{
				File.Delete(bin_filename);
			}
		}
		#endregion
	};
}