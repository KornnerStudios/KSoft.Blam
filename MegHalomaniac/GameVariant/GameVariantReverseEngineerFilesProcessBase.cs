using System.IO;

namespace MgloGui
{
	using KSoft;
	using MegaloModel = KSoft.Blam.Megalo.Model;

	abstract class GvarReverseEngineerFilesProcessBase : ReverseEngineerFilesProcessBase
	{
		protected MegaloModel.MegaloScriptModelTagElementStreamFlags mModelStreamFlags = 0;

		protected GvarReverseEngineerFilesProcessBase(MainWindowViewModel mainViewModel)
			: base(mainViewModel)
		{
			KSoft.Blam.Blob.GameEngineVariantBlob.RequireValidHashes = ViewModel.Flags.Test(MiscFlags.SkipVerification) == false;
		}

		protected override void GetReverseEngineeringFiles(
			string inputFileName,
			out string disassembledFileName,
			out string assembledFileName,
			out string outputFileName)
		{
			switch (FileReverseEngineeringMode)
			{
				case ReverseEngineeringMode.Disassemble:
					disassembledFileName = inputFileName;
					assembledFileName = Path.ChangeExtension(disassembledFileName, KSoft.Blam.RuntimeData.Variants.GameEngineVariant.kGameVariantXmlExtension);
					outputFileName = assembledFileName;
					break;

				case ReverseEngineeringMode.Assemble:
					assembledFileName = inputFileName;
					disassembledFileName = Path.ChangeExtension(assembledFileName, KSoft.Blam.RuntimeData.Variants.GameEngineVariant.kGameVariantBinExtension);
					outputFileName = disassembledFileName;
					break;

				default:
					disassembledFileName = assembledFileName = outputFileName = null;
					break;
			}
		}

		protected override void GetFinalReverseEngineeringFile(
			string outputFileName,
			out string finalOutputFileName)
		{
			finalOutputFileName = outputFileName;
			if (base.IgnoreOutputPaths)
				return;

			string desired_output_path = null;
			switch (FileReverseEngineeringMode)
			{
				case ReverseEngineeringMode.Disassemble:
					desired_output_path = ViewModel.GameVariantDisassemblerOutputPathOverride;
					break;

				case ReverseEngineeringMode.Assemble:
					desired_output_path = ViewModel.GameVariantAssemblerOutputPathOverride;
					break;
			}

			if (desired_output_path.IsNullOrEmpty())
				return;

			string final_file_name_and_extension = Path.GetFileName(finalOutputFileName);
			finalOutputFileName = Path.Combine(desired_output_path, final_file_name_and_extension);
		}
	};
}