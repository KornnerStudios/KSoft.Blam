
namespace KSoft.Blam.Engine
{
	/// <summary>Development stage of the engine</summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum EngineProductionStage : byte
	{
		Undefined,
		Alpha,
		Beta,
		/// <remarks>These are generally the "public beta" builds</remarks>
		Delta,
		/// <remarks>These are for when the source control is closed and bug smashing commences</remarks>
		Epsilon,
		/// <remarks>What's released to the public</remarks>
		Ship,
		/// <summary>Title Update</summary>
		TU,

		// #7 is unused

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}