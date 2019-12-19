using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace MgloGui
{
	using KSoft;
	using KBlam = KSoft.Blam;
	using MegaloModel = KSoft.Blam.Megalo.Model;

	public enum GvarAssemblerFlags
	{
		[Display(
			Name="Clear any Title Update configs",
			Description="So the engine will use the on-disc defaults (like bloom in Reach)")]
		ClearTitleUpdateData,
		[Display(
			Name="Clear any weapon tuning configs",
			Description="So the engine will use default values")]
		ClearWeaponTuning,
		[Browsable(false)] // #TODO_MGLO:
		[Display(
			Name="TBD",
			Description="TBD")]
		AddAllTeams,

		[Display(
			Name="Megalo: Read operation names ONLY",
			Description="Reads the condition/action name only, won't try to read a DBID")]
		ReadOperationNamesOnly,

		kNumberOf,
	};

	class GvarAssembler : GvarReverseEngineerFilesProcessBase
	{
		bool mClearTitleUpdateData = false;
		bool mClearWeaponTuning = false;

		int mFileOffset;
		int mEngineVersion;

		protected override ReverseEngineeringMode FileReverseEngineeringMode { get { return ReverseEngineeringMode.Assemble; } }

		public GvarAssembler(MainWindowViewModel mainViewModel)
			: base(mainViewModel)
		{
			InterpretFlags(ViewModel.GameVariantAssemblerFlags);

			mFileOffset = 0;
			Util.MarkUnusedVariable(ref mFileOffset); // #TODO_MGLO?

			if (mEngineVersion == 0 && mGameBuildAndTarget.Build.RevisionIndex.IsNotNone())
			{
				mEngineVersion = mGameBuildAndTarget.Build.Revision.Version;
			}
		}

		void InterpretFlags(KSoft.Collections.BitVector32 flags)
		{
			if (flags.Test(GvarAssemblerFlags.ClearTitleUpdateData))
			{
				mClearTitleUpdateData = true;
			}
			if (flags.Test(GvarAssemblerFlags.ClearWeaponTuning))
			{
				mClearWeaponTuning = true;
			}
			if (flags.Test(GvarAssemblerFlags.AddAllTeams))
			{
				// #TODO_MGLO:
			}
			if (flags.Test(GvarAssemblerFlags.ReadOperationNamesOnly))
			{
				EnumFlags.Add(ref mModelStreamFlags,
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseConditionTypeNames |
					MegaloModel.MegaloScriptModelTagElementStreamFlags.UseActionTypeNames);
			}
		}

		protected override void ReverseEngineerInputFile(string inputFile, string outputFile)
		{
			bool success = true;
			KBlam.RuntimeData.Variants.GameEngineVariant gev = null;
			success = success && EncodeLoadVariant(inputFile, out gev);
			success = success && EncodeVariantPreprocess(gev, outputFile);
			success = success && EncodeVariantBlf(outputFile, gev);
		}

		bool EncodeLoadVariant(string xmlFilename, out KBlam.RuntimeData.Variants.GameEngineVariant ev)
		{
			ev = new KBlam.RuntimeData.Variants.GameEngineVariant(mGameBuildAndTarget.Build);
			bool result = true;

			using (var xml = new KSoft.IO.XmlElementStream(xmlFilename))
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

		bool EncodeVariantPreprocess(
			KBlam.RuntimeData.Variants.GameEngineVariant gev, string filePath)
		{
			var mv = gev.TryGetMegaloVariant();

			if (mv == null)
				return true;

			if (mClearTitleUpdateData)
			{
				mv.ClearTitleUpdateData();
			}
			if (mClearWeaponTuning)
			{
				mv.ClearWeaponTunings();
			}

			// #TODO_MGLO: AddAllTeams

			return true;
		}

		bool EncodeVariantBlf(string filePath, KBlam.RuntimeData.Variants.GameEngineVariant gev)
		{
			bool result = true;

			using (var dst_fs = File.Open(filePath, System.IO.FileMode.Create, FileAccess.Write))
				result = EncodeVariantBlob(dst_fs, gev);

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

				var blob_system = mRequiredSystems.BlobSystemRef.System;

				if (fs.Length < blffile_length)
				{
					fs.SetLength(blffile_length);
				}

				blf_result = blf.OpenForWrite(fs, mFileOffset, blffile_length);
				if (blf_result.IsValid)
				{
					blf.UnderlyingStream.StreamMode = FileAccess.Write;

					int engine_version_to_write = mEngineVersion;
					if (megalo_variant != null)
					{
						if (megalo_variant.EngineVersion <= 0)
							megalo_variant.EngineVersion = engine_version_to_write;
						else
							engine_version_to_write = megalo_variant.EngineVersion;
					}

					var chdr = (KBlam.Blob.ContentHeaderBlob)blob_system.CreateObject(mGameBuildAndTarget, KBlam.Blob.WellKnownBlob.ContentHeader);
					chdr.ChangeData(gev.Variant.BaseVariant.Header, engine_version_to_write);

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
	};

	partial class MainWindowViewModel
	{
		#region GameVariantAssemblerFlags
		private static KSoft.WPF.BitVectorUserInterfaceData gGameVariantAssemblerFlagsUserInterfaceSource;
		public static KSoft.WPF.BitVectorUserInterfaceData GameVariantAssemblerFlagsUserInterfaceSource { get {
			if (gGameVariantAssemblerFlagsUserInterfaceSource == null)
				gGameVariantAssemblerFlagsUserInterfaceSource = KSoft.WPF.BitVectorUserInterfaceData.ForEnum(typeof(GvarAssemblerFlags));
			return gGameVariantAssemblerFlagsUserInterfaceSource;
		} }

		KSoft.Collections.BitVector32 mGameVariantAssemblerFlags;
		public KSoft.Collections.BitVector32 GameVariantAssemblerFlags
		{
			get { return mGameVariantAssemblerFlags; }
			set { this.SetFieldVal(ref mGameVariantAssemblerFlags, value); }
		}
		#endregion

		public string GameVariantAssemblerOutputPathOverride { get {
			if (Flags.Test(MiscFlags.IgnoreOutputPaths))
				return "";

			return Properties.Settings.Default.GvarAssemblyOutputPath;
		} }

		void InitializeGameVariantAssemblerState()
		{
		}

		void ProcessGameVariantXmlFiles(string[] xmlFiles)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				var assembler = new GvarAssembler(this);
				assembler.SetInputFiles(xmlFiles);
				assembler.ReverseEngineerInputFiles();
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					MessagesText += string.Format("Assembling game variantg XMLs failed {0}{1}",
						Environment.NewLine, t.Exception.GetOnlyExceptionOrAll());
				}

				FinishProcessing();
			}, scheduler);
		}
	};
}