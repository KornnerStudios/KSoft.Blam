
namespace KSoft.Blam.Engine
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum EngineBuildStringType
	{
		// Halo2 PC - Today
		Build_DateTime,

		// Halo1 - Halo2 Xbox
		Major_MonthDay_Build,
		// Stubbs-only
		Build,

		// 3

		/// <remarks>2 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}