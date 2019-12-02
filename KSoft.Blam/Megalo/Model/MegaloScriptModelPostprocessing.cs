using System;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		internal MegaloScriptModelCompilerState mCompilerState;
		internal MegaloScriptModelDecompilerState mDecompilerState;
		internal Action<MegaloScriptModel> CompilePostprocess;

		protected virtual void Compile()
		{
			mCompilerState.CompileTriggers();

			if (CompilePostprocess != null)
				CompilePostprocess(this);
		}
		protected virtual void Decompile()
		{
			mDecompilerState.DecompileTriggers();
			mDecompilerState.FixupUnionGroups();

			mDecompilerState.DecompileVariant();

			mDecompilerState.DecompileGameStatistics();
			mDecompilerState.DecompileHudWidgets();
			mDecompilerState.DecompileObjectFilters();
			mDecompilerState.DecompileCandySpawnerFilters();

			mDecompilerState.DecompileVariables();
		}

		void BeginCompile()
		{
			mCompilerState = new MegaloScriptModelCompilerState(this);

			Compile();
		}
		void EndCompile()
		{
			mCompilerState = null;
		}

		void BeginDecompile()
		{
			mDecompilerState = new MegaloScriptModelDecompilerState(this);

			Decompile();
		}
		void EndDecompile()
		{
			mDecompilerState = null;
		}
	};

	partial class MegaloScriptUnionGroup
	{
		/// <remarks>
		/// The decompiler adds the conditions to the group in reverse as it processes the conditions in reverse.
		/// This restores the 'left-to-right' 0-n order
		/// </remarks>
		internal void DecompilePostprocess()
		{
			if (mConditions.Count > 1)
				mConditions.Reverse();
		}
	};
}