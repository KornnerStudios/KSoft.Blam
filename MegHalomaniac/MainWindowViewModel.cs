using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KSoft;
using KSoft.Collections;

namespace MgloGui
{
	using KBlam = KSoft.Blam;

	public enum MiscFlags
	{
		[Display(
			Name="Ignore selected output paths",
			Description="Disassembled and assembled files will instead be output side-by-side with their input files")]
		IgnoreOutputPaths,

		[Display(
			Name="Don't overwrite existing files",
			Description="Files that already exist will not be overwritten")]
		DontOverwriteExistingFiles,

		[Browsable(false)] // #TODO_MGLO: remove once fully fleshed out
		[Display(
			Name="Verbose Output",
			Description="When performing operations, include any verbose details")]
		UseVerboseOutput,

		[Display(
			Name="Skip Verification",
			Description= "During variant disassembling, ignore checksums that appear to be wrong and would halt progress")]
		SkipVerification,

		kNumberOf,
	};

	internal partial class MainWindowViewModel
		: KSoft.ObjectModel.BasicViewModel
	{
		#region Flags
		private static KSoft.WPF.BitVectorUserInterfaceData gFlagsUserInterfaceSource;
		public static KSoft.WPF.BitVectorUserInterfaceData FlagsUserInterfaceSource { get {
			if (gFlagsUserInterfaceSource == null)
				gFlagsUserInterfaceSource = KSoft.WPF.BitVectorUserInterfaceData.ForEnum(typeof(MiscFlags));
			return gFlagsUserInterfaceSource;
		} }

		KSoft.Collections.BitVector32 mFlags;
		public KSoft.Collections.BitVector32 Flags
		{
			get { return mFlags; }
			set { this.SetFieldVal(ref mFlags, value); }
		}
		#endregion

		#region StatusText
		string mStatusText;
		public string StatusText
		{
			get { return mStatusText; }
			set { this.SetFieldObj(ref mStatusText, value); }
		}
		#endregion

		#region ProcessFilesHelpText
		string mProcessFilesHelpText;
		public string ProcessFilesHelpText
		{
			get { return mProcessFilesHelpText; }
			set { this.SetFieldObj(ref mProcessFilesHelpText, value); }
		}
		#endregion

		#region MessagesText
		string mMessagesText;
		public string MessagesText
		{
			get { return mMessagesText; }
			set { this.SetFieldObj(ref mMessagesText, value); }
		}
		#endregion

		#region IsProcessing
		bool mIsProcessing;
		public bool IsProcessing
		{
			get { return mIsProcessing; }
			set { this.SetFieldVal(ref mIsProcessing, value); }
		}
		#endregion

		#region Selected Game Build
		public string SelectedGameBuildNameSetting { get { return Properties.Settings.Default.SelectedGameBuild; } }

		public MgloBlamGameRequiredSystems SelectGameBuildRequiredSystems { get { return MgloBlamGameRequiredSystems.GetOrCreateFromSelectableGameBuildName(SelectedGameBuildNameSetting); } }

		public KBlam.Engine.EngineBuildHandle SelectedGameBuildHandle { get {
			var handle = KBlam.Engine.EngineBuildHandle.None;

			string selected_game_name = SelectedGameBuildNameSetting;
			KBlam.Engine.EngineBuildRevision build_revision = KBlam.Engine.EngineRegistry.TryParseExportedBuildName(selected_game_name);
			if (build_revision != null)
			{
				handle = build_revision.BuildHandle;
			}

			return handle;
		} }

		public KBlam.Engine.BlamEngineTargetHandle SelectedGameBuildAndTargetHandle { get {
			var build_and_target = KBlam.Engine.BlamEngineTargetHandle.None;

			var build_handle = SelectedGameBuildHandle;
			if (build_handle.IsNotNone)
			{
				build_and_target = build_handle.ToEngineTargetHandle();
			}

			return build_and_target;
		} }
		#endregion

		public MainWindowViewModel()
		{
			mFlags.Set(MiscFlags.SkipVerification);

			InitializeGameVariantAssemblerState();
			InitializeGameVariantDisassemblerState();

			ClearStatus();
			ClearProcessFilesHelpText();
			ClearMessages();
		}

		private void ClearStatus()
		{
			StatusText = "Ready...";
		}

		public void ClearProcessFilesHelpText()
		{
			ProcessFilesHelpText = "Drag-n-drop files";
		}

		private void ClearMessages()
		{
			MessagesText = "";
		}

		public enum AcceptedFileType
		{
			Unaccepted,
			Directory,
			Bin,
			GvarXml,

			kNumberOf
		};
		public struct AcceptedFilesResults
		{
			public BitVector32 AcceptedFileTypes;
			public int FilesCount;
		};
		public static AcceptedFilesResults DetermineAcceptedFiles(string[] files, BitVector32 miscFlags)
		{
			var results = new AcceptedFilesResults();

			if (files == null || files.Length == 0)
				return results;

			results.FilesCount = files.Length;

			foreach (string path in files)
			{
				if (System.IO.Directory.Exists(path))
				{
					results.AcceptedFileTypes.Set(AcceptedFileType.Directory);
					continue;
				}

				string ext = System.IO.Path.GetExtension(path);
				if (string.IsNullOrEmpty(ext)) // extension-less file
				{
					results.AcceptedFileTypes.Set(AcceptedFileType.Unaccepted);
					continue;
				}

				switch (ext)
				{
					case KSoft.Blam.RuntimeData.Variants.GameEngineVariant.kGameVariantBinExtension:
						results.AcceptedFileTypes.Set(AcceptedFileType.Bin);
						break;
					case KSoft.Blam.RuntimeData.Variants.GameEngineVariant.kGameVariantXmlExtension:
						results.AcceptedFileTypes.Set(AcceptedFileType.GvarXml);
						break;
				}
			}

			return results;
		}

		public bool AcceptsFiles(string[] files)
		{
			var results = DetermineAcceptedFiles(files, this.Flags);
			if (results.FilesCount == 0)
				return false;

			if (results.AcceptedFileTypes.Cardinality != 0 && !results.AcceptedFileTypes.Test(AcceptedFileType.Unaccepted))
			{
				if (AcceptsFilesInternal(results, files))
					return true;
			}

			ProcessFilesHelpText = "Unacceptable file or group of files";
			return false;
		}
		public void ProcessFiles(string[] files)
		{
			var results = DetermineAcceptedFiles(files, this.Flags);
			if (results.FilesCount == 0)
				return;

			if (results.AcceptedFileTypes.Cardinality != 0 && !results.AcceptedFileTypes.Test(AcceptedFileType.Unaccepted))
			{
				if (ProcessFilesInternal(results, files))
					ProcessFilesHelpText = "";
			}
		}

		private bool AcceptsFilesInternal(AcceptedFilesResults results, string[] files)
		{
			foreach (int bitIndex in results.AcceptedFileTypes.SetBitIndices)
			{
				var type = (AcceptedFileType)bitIndex;
				switch (type)
				{
					case AcceptedFileType.Bin:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessFilesHelpText = "Disassemble Game Variant Bin(s)";
							return true;
						}
						break;
					}

					case AcceptedFileType.GvarXml:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessFilesHelpText = "Assemble Game Variant Bins(s) from XML source";
							return true;
						}
						break;
					}

					case AcceptedFileType.Directory:
					{
#if false // #TODO_MGLO
							// #TODO_MGLO: Detect CTRL key to do XML->BIN?

							if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessFilesHelpText = "BIN->MGLOXML (in directories)";
							return true;
						}
#endif
							break;
					}
				}
			}

			return false;
		}
		private bool ProcessFilesInternal(AcceptedFilesResults results, string[] files)
		{
			foreach (int bitIndex in results.AcceptedFileTypes.SetBitIndices)
			{
				var type = (AcceptedFileType)bitIndex;
				switch (type)
				{
					case AcceptedFileType.Bin:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessGameVariantBinFiles(files);
							return true;
						}
						break;
					}

					case AcceptedFileType.GvarXml:
					{
						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							ProcessGameVariantXmlFiles(files);
							return true;
						}
						break;
					}

					case AcceptedFileType.Directory:
					{
						// #TODO_MGLO: Detect CTRL key to do XML->BIN?

						if (results.AcceptedFileTypes.Cardinality == 1)
						{
							// #TODO_MGLO
							//BinToGameVariantXmlInDirectories(files);
							throw new NotImplementedException(type.ToString());
							//return true;
						}
						break;
					}
				}
			}

			return false;
		}

		private void FinishProcessing()
		{
			ClearStatus();
			ClearProcessFilesHelpText();
			IsProcessing = false;
		}

		public void RefreshForCurrentlySelectedGameBuild()
		{
			if (SelectedGameBuildNameSetting.IsNullOrEmpty())
				return;

			ClearMessages();
			ClearStatus();
			IsProcessing = true;

			StatusText = string.Format("Loading {0}...", SelectedGameBuildNameSetting);

			var task = Task.Run(() =>
			{
				var required_systems = SelectGameBuildRequiredSystems;
				if (required_systems != null)
				{
					required_systems.LoadSystems();
					//System.Threading.Thread.Sleep(3 * 1000);
				}
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					MessagesText += string.Format("Loading {0} systems failed {1}{2}",
						SelectedGameBuildNameSetting,
						Environment.NewLine,
						t.Exception.GetOnlyExceptionOrAll());
				}

				FinishProcessing();
			}, scheduler);
		}
	};
}
