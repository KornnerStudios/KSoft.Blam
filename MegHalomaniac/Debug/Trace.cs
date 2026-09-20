using Diag = System.Diagnostics;

namespace MgloGui.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		/// <summary>Trace source for MegHalomaniac UI diagnostics</summary>
		public static Diag.TraceSource MgloGui { get; } =	new Diag.TraceSource("PhxGui", Diag.SourceLevels.All);
	};
}
