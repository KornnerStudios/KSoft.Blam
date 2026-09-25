using Diag = System.Diagnostics;

namespace KSoft.Tool.Debug;

internal static class Trace
{
	public static Diag.TraceSource BlamTool { get; } = new(Program.TraceCategoryName, Diag.SourceLevels.All);
}
