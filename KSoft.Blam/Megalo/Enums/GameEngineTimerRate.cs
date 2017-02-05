
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum GameEngineTimerRate : byte
	{
		None,

		#region Minus
		Minus10p,
		Minus25p,
		Minus50p,
		Minus75p,
		Minus100p,
		Minus125p,
		Minus150p,
		Minus175p,
		Minus200p,
		Minus300p,
		Minus400p,
		Minus500p,
		Minus1000p,
		#endregion
		#region Plus
		Plus10p,
		Plus25p,
		Plus50p,
		Plus75p,
		Plus100p,
		Plus125p,
		Plus150p,
		Plus175p,
		Plus200p,
		Plus300p,
		Plus400p,
		Plus500p,
		Plus1000p,
		#endregion

		/// <remarks>5 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}