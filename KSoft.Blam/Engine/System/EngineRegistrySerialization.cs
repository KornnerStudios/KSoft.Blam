using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Engine
{
	partial class EngineRegistry
	{
		static string GetEngineRootPath(BlamEngine engine)
		{
			Contract.Assert(engine != null);

			return Path.Combine(@"Games\", engine.Name);
		}
		static string GetTagElementStreamPath(BlamEngine engine, string fileName)
		{
			Contract.Assert(engine != null);
			Contract.Assert(!string.IsNullOrEmpty(fileName));

			return Path.Combine(GetEngineRootPath(engine), fileName);
		}
		internal static bool IsValidTagElmentStreamFile(BlamEngine engine, string fileName)
		{
			Contract.Requires(engine != null);
			Contract.Requires(!string.IsNullOrEmpty(fileName));

			string path = GetTagElementStreamPath(engine, fileName);

			return File.Exists(path);
		}

		static dynamic OpenRegistryTagElementStream(FileAccess streamMode = FileAccess.Read)
		{
			Contract.Requires<FileNotFoundException>(File.Exists(kRegistryFilePath),
				"Can't initialize the EngineRegistry, need the following file: " + kRegistryFilePath);

			var stream = IO.TagElementStreamFactory.Open(kRegistryFilePath);
			stream.StreamMode = streamMode;

			return stream;
		}

		static dynamic OpenEngineTagElementStream(BlamEngine engine, FileAccess streamMode = FileAccess.Read)
		{
			Contract.Assert(engine != null);

			string engine_stream_path = GetTagElementStreamPath(engine, engine.Name);
			engine_stream_path += ".xml";

			if (!File.Exists(engine_stream_path))
				throw new FileNotFoundException(string.Format(
					"Can't initialize the {0} engine, need the following file: {1}",
					engine.Name,
					engine_stream_path));

			var stream = IO.TagElementStreamFactory.Open(engine_stream_path);
			stream.StreamMode = streamMode;

			return stream;
		}
		internal static dynamic OpenEngineSystemTagElementStream(BlamEngine engine, Values.KGuid systemGuid, string externFileName,
			FileAccess streamMode = FileAccess.Read)
		{
			Contract.Assert(engine != null);

			string extern_stream_path = GetTagElementStreamPath(engine, externFileName);

			if (!File.Exists(extern_stream_path))
				throw new FileNotFoundException(string.Format(
					"Can't initialize the {0} engine's {1} system, need the following file: {2}",
					engine.Name,
					systemGuid.ToString(Values.KGuid.kFormatHyphenated),
					extern_stream_path));

			var stream = IO.TagElementStreamFactory.Open(extern_stream_path);
			stream.StreamMode = streamMode;

			return stream;
		}

		static void SerializePrototypes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// #REVIEW_BLAM: we don't current validate the series name
			string series = kSeriesName;
			s.StreamAttribute("series", ref series);

			using (var bm = s.EnterCursorBookmarkOpt("Engines", gEngines, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamElements("Engine", gEngines, (object)null,
					BlamEngine.SerializePrototype);
		}
		static void SerializeTargets<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Targets"))
			{
				using (var bm = s.EnterCursorBookmarkOpt("Platforms", gTargetPlatforms, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Platform", gTargetPlatforms);

				using (var bm = s.EnterCursorBookmarkOpt("ResourceModels", gResourceModels, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamElements("Model", gResourceModels);
			}
		}

		internal static bool SerializeResourceModelId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int resourceModelId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref resourceModelId, null,
					ResourceModelIdResolver, ResourceModelNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
					resourceModelId = TypeExtensions.kNone;
			}
			else
			{
				s.StreamAttributeIdAsString(attributeName, ref resourceModelId, null,
					ResourceModelIdResolver, ResourceModelNameResolver);
			}

			return streamed;
		}
	};
}