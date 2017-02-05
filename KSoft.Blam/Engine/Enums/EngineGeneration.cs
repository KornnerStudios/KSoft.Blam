
namespace KSoft.Blam.Engine
{
	/// <summary>Development time period of the engine</summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum EngineGeneration : byte
	{
		/// <summary>Halo 1, Stubbs</summary>
		First,
		/// <summary>Halo2</summary>
		Second,
		/// <summary>Halo3, ODST, Reach, Halo4</summary>
		Third,
		/// <summary>Halo5?</summary>
		Forth,

		// 4 to 7 left

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}