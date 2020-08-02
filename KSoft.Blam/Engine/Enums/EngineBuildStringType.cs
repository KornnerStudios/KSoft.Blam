using System.Diagnostics.CodeAnalysis;

namespace KSoft.Blam.Engine
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum EngineBuildStringType
	{
		// Halo2 PC - Today
		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		Build_DateTime,

		// Halo1 - Halo2 Xbox
		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		Major_MonthDay_Build,
		// Stubbs-only
		Build,

		// 3

		/// <remarks>2 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}
