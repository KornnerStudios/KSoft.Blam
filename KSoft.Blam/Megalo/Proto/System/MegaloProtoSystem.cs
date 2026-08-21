using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KSoft.Blam.Megalo.Proto
{
	using AllDatabasesTasksTuple = ValueTuple
			< Task<MegaloStaticDatabase>
			, Task<MegaloScriptDatabase>
			>;

	[Engine.EngineSystem(KeepExternsLoaded=true)]
	public sealed class MegaloProtoSystem
		: Engine.EngineSystemBase
	{
		public static Values.KGuid SystemGuid { get; } = new Values.KGuid("F3047D03-8474-44C6-BB9E-49745454BD3D");

		// #NOTE_BLAM: there should only ever actually be 1 or 2 entries (beta and release)
		const int kBuildProtoFilesExpectedCapacity = 2;
		readonly Dictionary<Engine.EngineBuildHandle, BuildProtoFiles> mBuildProtoFiles
			 = new(kBuildProtoFilesExpectedCapacity);

		readonly Dictionary<string, MegaloStaticDatabase> mLoadedStaticDbs = new();
		readonly Dictionary<string, MegaloScriptDatabase> mLoadedScriptDbs = new();

		public bool IsSpecificBuildSupported(Engine.EngineBuildHandle forBuild)
		{
			return IsSpecificBuildSupported(forBuild, out Engine.EngineBuildHandle /*actual_build*/_);
		}
		public bool IsSpecificBuildSupported(Engine.EngineBuildHandle forBuild, out Engine.EngineBuildHandle actualBuild)
		{
			var files = BuildProtoFiles.Empty;
			forBuild.TryGetValue(mBuildProtoFiles, ref files, out actualBuild);

			return !files.IsEmpty;
		}

		#region GetDatabasePath
		private delegate string GetDatabasePathFunc(Engine.EngineBuildHandle forBuild, out Engine.EngineBuildHandle actualBuild);

		public string GetStaticDatabasePath(Engine.EngineBuildHandle forBuild, out Engine.EngineBuildHandle actualBuild)
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			var files = BuildProtoFiles.Empty;
			forBuild.TryGetValue(mBuildProtoFiles, ref files, out actualBuild);

			return files.StaticDatabaseFile;
		}
		public string GetMegaloDatabasePath(Engine.EngineBuildHandle forBuild, out Engine.EngineBuildHandle actualBuild)
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			var files = BuildProtoFiles.Empty;
			forBuild.TryGetValue(mBuildProtoFiles, ref files, out actualBuild);

			return files.MegaloDatabaseFile;
		}
		#endregion

		#region GetDatabase
		static void LoadDatabase<T>(T db, string path)
			where T : class, IO.ITagElementStringNameStreamable
		{
			using (var tag_stream = IO.TagElementStreamFactory.Open(path, System.IO.FileAccess.Read))
			{
				tag_stream.StreamMode = System.IO.FileAccess.Read;

				db.Serialize(tag_stream);
			}
		}
		static async Task<T> GetDatabaseAsync<T>(Engine.EngineBuildHandle forBuild,
			string dbTypeName,
			GetDatabasePathFunc getPathFunc,
			Dictionary<string, T> loadedDbs,
			Func<Engine.EngineBuildHandle, T> ctor)
			where T : class, IO.ITagElementStringNameStreamable
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}
			ArgumentException.ThrowIfNullOrEmpty(dbTypeName);
			ArgumentNullException.ThrowIfNull(getPathFunc);
			ArgumentNullException.ThrowIfNull(loadedDbs);
			ArgumentNullException.ThrowIfNull(ctor);

			string path = getPathFunc(forBuild, out Engine.EngineBuildHandle actual_build);
#pragma warning disable IDE0270 // Use coalesce expression
			if (path == null)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Tried to get the megalo {0} database for {1} when a build file wasn't defined for it",
					dbTypeName,
					forBuild.ToDisplayString()));
			}
#pragma warning restore IDE0270 // Use coalesce expression

			T db;
			lock (loadedDbs)
			{
				loadedDbs.TryGetValue(path, out db);
			}

			if (db == null)
			{
				db = ctor(actual_build);
				lock (loadedDbs)
				{
					loadedDbs[path] = db;
				}

				await Task.Run(() => LoadDatabase(db, path)).ConfigureAwait(false);
			}

			return db;
		}

		public Task<MegaloStaticDatabase> GetStaticDatabaseAsync(Engine.EngineBuildHandle forBuild)
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			return GetDatabaseAsync(forBuild, "static",
				GetStaticDatabasePath, mLoadedStaticDbs,
				actualBuild => {
					var mdb = GetMegaloDatabaseAsync(actualBuild);
					return new MegaloStaticDatabase(actualBuild, mdb.Result.Limits);
				});
		}
		public Task<MegaloScriptDatabase> GetMegaloDatabaseAsync(Engine.EngineBuildHandle forBuild)
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			return GetDatabaseAsync(forBuild, "script",
				GetMegaloDatabasePath, mLoadedScriptDbs,
				actualBuild => new MegaloScriptDatabase(actualBuild));
		}
		public static bool OutputMegaloDatabasePostprocessErrorTextToConsole { get; set; } = false;
		static void PostprocessMegaloDatabase(MegaloScriptDatabase db, MegaloStaticDatabase associatedStaticDb)
		{
			System.IO.TextWriter errorOutput = null;
			if (Blam.Program.RunningUnitTests || OutputMegaloDatabasePostprocessErrorTextToConsole)
			{
				errorOutput = Console.Out;
			}

			db.Postprocess(associatedStaticDb, errorOutput);
		}

		public AllDatabasesTasksTuple GetAllDatabasesAsync(Engine.EngineBuildHandle forBuild)
		{
			if (forBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(forBuild));
			}

			var static_db_task = GetStaticDatabaseAsync(forBuild);
			var megalo_db_task = GetMegaloDatabaseAsync(forBuild);

			return new AllDatabasesTasksTuple
				( static_db_task
				, megalo_db_task
				);
		}

		public static void PrepareDatabasesForUse(MegaloStaticDatabase staticDb, MegaloScriptDatabase scriptDb)
		{
			ArgumentNullException.ThrowIfNull(staticDb);
			ArgumentNullException.ThrowIfNull(scriptDb);
			if (scriptDb.StaticDatabase != null)
			{
				throw new ArgumentException("Script db already had a static db reference set", nameof(scriptDb));
			}

			PostprocessMegaloDatabase(scriptDb, staticDb);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		protected override void SerializeExternBody<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			using (s.EnterCursorBookmark("BuildFiles"))
			{
				s.StreamableElements("Files",
					mBuildProtoFiles, this.RootBuildHandle,
					Blam.Engine.EngineBuildHandle.SerializeWithBaseline);
			}
		}
		#endregion

		protected override void UnloadExternsData()
		{
			base.UnloadExternsData();

			mLoadedStaticDbs.Clear();
			mLoadedScriptDbs.Clear();
		}

		// For simple queries only! Don't hold on to the value
		internal static Task<MegaloScriptDatabase> GetScriptDatabaseAsync(Engine.EngineBuildHandle gameBuild)
		{
			if (gameBuild.IsNone)
			{
				throw new ArgumentNoneException(nameof(gameBuild));
			}

			using (var system_ref = Blam.Engine.EngineRegistry.GetSystem<MegaloProtoSystem>(gameBuild))
			{
				var system = system_ref.System;
				return system.GetMegaloDatabaseAsync(gameBuild);
			}
		}
	};
}
