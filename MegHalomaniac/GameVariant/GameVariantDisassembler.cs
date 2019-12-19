using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MgloGui
{
	using KSoft;
	using KBlam = KSoft.Blam;
	using MegaloModel = KSoft.Blam.Megalo.Model;

	public enum GvarDisassemblerFlags
	{
		[Display(
			Name="Always write 'default' data. EXPERIMENTAL!",
			Description=
				"Even when values are set to their defaults, they will be written out to the disassembly.\n"+
				"This is highly experimental, and skips some elements of the game variant (like the MegaloScript).")]
		AlwaysWriteDefaultData,

		[Display(
			Name="Megalo: Write operation names ONLY",
			Description=
				"Writes the condition/action name only, won't include their DBID")]
		WriteOperationNamesOnly,
		[Display(
			Name="Megalo: Write param names",
			Description=
				"Writes out the names of parameters as documented in the engine's megalo script DB.\n" +
				"Can be useful when trying to understand condition/action inputs and outputs.\n" +
				"NOTE: They WILL NOT be read back when re-assembling.")]
		WriteOperationParameterNames,
		[Display(
			Name="Megalo: Write param types",
			Description=
				"Writes out the parameter's type name too.\n" +
				"NOTE: They WILL NOT be read back when re-assembling")]
		WriteOperationParameterContexts,

		[Display(
			Name="Megalo: Don't write enum/index names",
			Description="Human-readable names will not be written, only their literal values (integers)")]
		DoNotWriteEnumOrIndexNames,
		[Display(
			Name="Megalo: Write internal object IDs",
			Description=
				"Internal object IDs are generally the index at which things appeared within the binary")]
		WriteWithObjectIds,
		[Display(
			Name="Megalo: Do not embed child data",
			Description=
				"Embedding flattens the DB hierarchy, while turning it off keeps data in self contained groups of elements.\n" +
				"Not useful to the common user.")]
		DoNotEmbedObjects,

		[Browsable(false)] // #TODO_MGLO:
		[Display(
			Name="TBD",
			Description="TBD")]
		TryToPort,

		kNumberOf,
	};

	class GvarDisassembler : GvarReverseEngineerFilesProcessBase
	{
		bool mIgnoreWritePredicates;

		int mFileOffset;

		protected override ReverseEngineeringMode FileReverseEngineeringMode { get { return ReverseEngineeringMode.Disassemble; } }

		public GvarDisassembler(MainWindowViewModel mainViewModel)
			: base(mainViewModel)
		{
			mModelStreamFlags =
				MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjects |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.UseEnumNames |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.UseIndexNames |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteConditionTypeNames |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteActionTypeNames |

				MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjectsWriteSansIds
				;

			mIgnoreWritePredicates = false;

			InterpretFlags(ViewModel.GameVariantDisassmblerFlags);

			mFileOffset = 0;
			Util.MarkUnusedVariable(ref mFileOffset); // #TODO_MGLO?
		}

		void InterpretFlags(KSoft.Collections.BitVector32 flags)
		{
			bool using_op_names = false;

			if (flags.Test(GvarDisassemblerFlags.WriteOperationNamesOnly))
			{
				EnumFlags.Add(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseConditionTypeNames |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseActionTypeNames);

				EnumFlags.Remove(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteConditionTypeNames |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteActionTypeNames);

				using_op_names = true;
			}
			if (flags.Test(GvarDisassemblerFlags.WriteOperationParameterNames))
			{
				EnumFlags.Add(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteParamNames);
			}
			if (flags.Test(GvarDisassemblerFlags.WriteOperationParameterContexts))
			{
				EnumFlags.Add(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.WriteParamKinds);
			}
			if (flags.Test(GvarDisassemblerFlags.TryToPort) &&
				mGameBuildAndTarget.Build.IsWithinSameBranch(KBlam.Engine.EngineRegistry.EngineBranchHaloReach))
			{
				if (using_op_names)
				{
					Debug.Trace.MgloGui.TraceInformation("Ignoring TryToPort since you have me writing operation names");
				}
				else
				{
					EnumFlags.Add(ref mModelStreamFlags,
						MegaloModel.MegaloScriptModelTagElementStreamFlags.TryToPort);
				}
			}
			if (flags.Test(GvarDisassemblerFlags.DoNotWriteEnumOrIndexNames))
			{
				EnumFlags.Remove(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseEnumNames |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseIndexNames);
			}
			if (flags.Test(GvarDisassemblerFlags.WriteWithObjectIds))
			{
				EnumFlags.Remove(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjectsWriteSansIds);
			}
			if (flags.Test(GvarDisassemblerFlags.DoNotEmbedObjects))
			{
				EnumFlags.Remove(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjects |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjectsWriteSansIds);
			}
			if (flags.Test(GvarDisassemblerFlags.AlwaysWriteDefaultData))
			{
				mIgnoreWritePredicates = true;
			}
		}

		protected override void ReverseEngineerInputFile(string inputFile, string outputFile)
		{
			KBlam.RuntimeData.Variants.GameEngineVariant gev = null;
			if (DecodeVariantBlf(inputFile, out gev))
			{
				DecodeSaveVariant(gev, outputFile);
			}
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
					throw new InvalidDataException(string.Format("Error: not enough data starting at offset 0x{0} to be a variant we support",
						mFileOffset.ToString("X8")));
					//result = false;
				}
				else
				{
					result = DecodeVariantBlob(dst_fs, out gev);
				}
			}

			return result;
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

				var blob_system = mRequiredSystems.BlobSystemRef.System;

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
				throw new InvalidDataException(string.Format("Error: Failed to decode variant file{0}{1}", System.Environment.NewLine,
					blf_result.BuildErrorMessage()));
			}
			else if (gevb == null)
			{
				throw new InvalidDataException("Error: Not a game variant file");
			}
			else if (KBlam.Blob.GameEngineVariantBlob.RequireValidHashes && gevb.InvalidData)
			{
				throw new InvalidDataException("Error: Game variant file's bitstream is corrupt");
			}
			else
			{
				gev = gevb.Data;
			}

			return blf_result.IsValid;
		}

		void DecodeSaveVariant(KBlam.RuntimeData.Variants.GameEngineVariant gev, string xmlFilename)
		{
			var megalo_variant = gev.TryGetMegaloVariant();
			if (megalo_variant != null)
			{
				megalo_variant.TagElementStreamSerializeFlags =
					KBlam.RuntimeData.Variants.GameEngineMegaloVariantTagElementStreamFlags.UseStringTableNames |
					KBlam.RuntimeData.Variants.GameEngineMegaloVariantTagElementStreamFlags.UseUserOptionNames;
				megalo_variant.EngineDefinition.TagElementStreamSerializeFlags =
					base.mModelStreamFlags;
			}

			using (var xml = KSoft.IO.XmlElementStream.CreateForWrite("GameVariant"))
			{
				xml.StreamMode = FileAccess.Write;
				xml.IgnoreWritePredicates = this.mIgnoreWritePredicates;

				gev.Serialize(xml);

				using (var sw = new System.IO.StreamWriter(xmlFilename, false, System.Text.Encoding.UTF8))
					xml.Document.Save(sw);
			}
		}
	};

	partial class MainWindowViewModel
	{
		#region GameVariantDisassmblerFlags
		private static KSoft.WPF.BitVectorUserInterfaceData gGameVariantDisassmblerFlagsUserInterfaceSource;
		public static KSoft.WPF.BitVectorUserInterfaceData GameVariantDisassmblerFlagsUserInterfaceSource { get {
			if (gGameVariantDisassmblerFlagsUserInterfaceSource == null)
				gGameVariantDisassmblerFlagsUserInterfaceSource = KSoft.WPF.BitVectorUserInterfaceData.ForEnum(typeof(GvarDisassemblerFlags));
			return gGameVariantDisassmblerFlagsUserInterfaceSource;
		} }

		KSoft.Collections.BitVector32 mGameVariantDisassmblerFlags;
		public KSoft.Collections.BitVector32 GameVariantDisassmblerFlags
		{
			get { return mGameVariantDisassmblerFlags; }
			set { this.SetFieldVal(ref mGameVariantDisassmblerFlags, value); }
		}
		#endregion

		public string GameVariantDisassemblerOutputPathOverride { get {
			if (Flags.Test(MiscFlags.IgnoreOutputPaths))
				return "";

			return Properties.Settings.Default.GvarDisassemblyOutputPath;
		} }

		void InitializeGameVariantDisassemblerState()
		{
		}

		void ProcessGameVariantBinFiles(string[] binFiles)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				var disassembler = new GvarDisassembler(this);
				disassembler.SetInputFiles(binFiles);
				disassembler.ReverseEngineerInputFiles();
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					MessagesText += string.Format("Disassembling game variantg BINs failed {0}{1}",
						Environment.NewLine, t.Exception.GetOnlyExceptionOrAll());
				}

				FinishProcessing();
			}, scheduler);
		}
	};
}