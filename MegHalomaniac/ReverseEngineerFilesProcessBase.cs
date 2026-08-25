using KSoft;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MgloGui
{
	using KBlam = KSoft.Blam;

	enum ReverseEngineeringMode
	{
		Disassemble,
		Assemble,
	};

	abstract class ReverseEngineerFilesProcessBase
	{
		public MainWindowViewModel ViewModel;
		public System.Windows.Threading.Dispatcher Dispatcher;
		public bool IgnoreOutputPaths;
		public bool DontOverwriteExistingFiles;

		protected KBlam.Engine.BlamEngineTargetHandle mGameBuildAndTarget = KBlam.Engine.BlamEngineTargetHandle.None;
		protected MgloBlamGameRequiredSystems mRequiredSystems;
		private List<string> mInputFiles = null!;

		protected abstract ReverseEngineeringMode FileReverseEngineeringMode { get; }

		public ReverseEngineerFilesProcessBase(MainWindowViewModel mainViewModel)
		{
			ViewModel = mainViewModel;
			Dispatcher = System.Windows.Application.Current.Dispatcher;
			DontOverwriteExistingFiles = ViewModel.Flags.Test(MiscFlags.DontOverwriteExistingFiles);
			IgnoreOutputPaths = ViewModel.Flags.Test(MiscFlags.IgnoreOutputPaths);

			mGameBuildAndTarget = ViewModel.SelectedGameBuildAndTargetHandle;
			mRequiredSystems = ViewModel.SelectGameBuildRequiredSystems;
		}

		public void SetInputFiles(string[] files)
		{
			mInputFiles = new List<string>(files);
		}

		#region Notify MessagesText utils
		void NotifyInputFileSkipped(string inputFile)
		{
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Create(CultureInfo.CurrentCulture,
						$"Skipped due to existing output {inputFile}{Environment.NewLine}");
				}));
		}

		void NotifyOutputFileReadOnly(string inputFile, string outputFile)
		{
			Util.MarkUnusedVariable(ref outputFile);

			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Create(CultureInfo.CurrentCulture,
						$"Skipped due to read-only output {inputFile}{Environment.NewLine}");
				}));
		}

		void NotifyInputFileException(string inputFile, Exception e)
		{
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
				new Action(() =>
				{
					ViewModel.MessagesText += string.Create(CultureInfo.CurrentCulture,
						$"EXCEPTION {inputFile}{Environment.NewLine}{e}");
				}));
		}
		#endregion

		protected abstract void GetReverseEngineeringFiles(
			string inputFileName,
			out string disassembledFileName,
			out string assembledFileName,
			out string outputFileName);
		protected abstract void GetFinalReverseEngineeringFile(
			string outputFileName,
			out string finalOutputFileName);

		protected abstract void ReverseEngineerInputFile(string inputFile, string outputFile);

		public void ReverseEngineerInputFiles()
		{
			var p = Parallel.ForEach(mInputFiles, f =>
			{
				try
				{
					GetReverseEngineeringFiles(f, out string disasm_file, out string asm_file, out string output_file);
					GetFinalReverseEngineeringFile(output_file, out string final_output_file);

					var output_info = new System.IO.FileInfo(final_output_file);
					if (output_info.Exists)
					{
						if (DontOverwriteExistingFiles)
						{
							NotifyInputFileSkipped(f);
							return;
						}
						else
						{
							if ((output_info.Attributes & System.IO.FileAttributes.ReadOnly) != 0)
							{
								NotifyOutputFileReadOnly(f, final_output_file);
								return;
							}
						}
					}

					ReverseEngineerInputFile(f, final_output_file);
				}
				catch (Exception e)
				{
					NotifyInputFileException(f, e);
				}
			});
		}
	};
}
