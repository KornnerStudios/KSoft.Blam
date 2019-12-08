using Diag = System.Diagnostics;

namespace KSoft.Blam.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	[System.Reflection.Obfuscation(Exclude=false)]
	internal static class Trace
	{
		public static Diag.TraceSource	Assembly		{ get; } = new	Diag.TraceSource("KSoft.Blam",					Diag.SourceLevels.All);
		public static Diag.TraceSource	Blob			{ get; } = new	Diag.TraceSource("KSoft.Blam.Blob",				Diag.SourceLevels.All);
		//public static Diag.TraceSource	Cache			{ get; } = new	Diag.TraceSource("KSoft.Blam.Cache",			Diag.SourceLevels.All);

		public static Diag.TraceSource	Engine			{ get; } = new	Diag.TraceSource("KSoft.Blam.Engine",			Diag.SourceLevels.All);
		public static Diag.TraceSource	Games			{ get; } = new	Diag.TraceSource("KSoft.Blam.Games",			Diag.SourceLevels.All);

		//public static Diag.TraceSource	ResourceData	{ get; } = new	Diag.TraceSource("KSoft.Blam.ResourceData",		Diag.SourceLevels.All);
		public static Diag.TraceSource	RuntimeData		{ get; } = new	Diag.TraceSource("KSoft.Blam.RuntimeData",		Diag.SourceLevels.All);
		//public static Diag.TraceSource	TagInterface	{ get; } = new	Diag.TraceSource("KSoft.Blam.TagInterface",		Diag.SourceLevels.All);
	};
}