using System;
using Diag = System.Diagnostics;

namespace KSoft.Blam.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	[System.Reflection.Obfuscation(Exclude=false)]
	internal static class Trace
	{
		static Diag.TraceSource kBlam
			, kBlob
			//, kCache

			, kEngine
			//, kGames

			//, kResourceData
			//, kRuntimeData
			//, kTagInterface
			;

		static Trace()
		{
			kBlam = new				Diag.TraceSource("KSoft.Blam",					Diag.SourceLevels.All);
			kBlob = new				Diag.TraceSource("KSoft.Blam.Blob",				Diag.SourceLevels.All);
			//kCache = new			Diag.TraceSource("KSoft.Blam.Cache",			Diag.SourceLevels.All);
			kEngine = new			Diag.TraceSource("KSoft.Blam.Engine",			Diag.SourceLevels.All);

			//kGames = new			Diag.TraceSource("KSoft.Blam.Games",			Diag.SourceLevels.All);

			//kResourceData = new		Diag.TraceSource("KSoft.Blam.ResourceData",		Diag.SourceLevels.All);
			//kRuntimeData = new		Diag.TraceSource("KSoft.Blam.RuntimeData",		Diag.SourceLevels.All);
			//kTagInterface = new		Diag.TraceSource("KSoft.Blam.TagInterface",		Diag.SourceLevels.All);
		}

		public static Diag.TraceSource	Assembly		{ get { return kBlam; } }
		public static Diag.TraceSource	Blob			{ get { return kBlob; } }
		//public static Diag.TraceSource	Cache			{ get { return kCache; } }

		public static Diag.TraceSource	Engine			{ get { return kEngine; } }
		//public static Diag.TraceSource	Games			{ get { return kGames; } }

		//public static Diag.TraceSource	ResourceData	{ get { return kResourceData; } }
		//public static Diag.TraceSource	RuntimeData		{ get { return kRuntimeData; } }
		//public static Diag.TraceSource	TagInterface	{ get { return kTagInterface; } }
	};
}