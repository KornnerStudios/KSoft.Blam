using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

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
		readonly Dictionary<Engine.EngineBuildHandle, BuildProtoFiles> mBuildProtoFiles;

		readonly Dictionary<string, MegaloStaticDatabase> mLoadedStaticDbs;
		readonly Dictionary<string, MegaloScriptDatabase> mLoadedScriptDbs;

		internal MegaloProtoSystem()
		{
			mBuildProtoFiles = new Dictionary<Engine.EngineBuildHandle, BuildProtoFiles>(kBuildProtoFilesExpectedCapacity);

			mLoadedStaticDbs = new Dictionary<string, MegaloStaticDatabase>();
			mLoadedScriptDbs = new Dictionary<string, MegaloScriptDatabase>();
		}

		#region GetDatabasePath
		private delegate string GetDatabasePathFunc(Engine.EngineBuildHandle forBuild, out Engine.EngineBuildHandle actualBuild);

		public string GetStaticDatabasePath(Engine.EngineBuildHandle forBuild, out Engine.EngineBuildHandle actualBuild)
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);

			var files = BuildProtoFiles.Empty;
			forBuild.TryGetValue(mBuildProtoFiles, ref files, out actualBuild);

			return files.StaticDatabaseFile;
		}
		public string GetMegaloDatabasePath(Engine.EngineBuildHandle forBuild, out Engine.EngineBuildHandle actualBuild)
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);

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
			Contract.Requires/*<ArgumentNullException>*/(!forBuild.IsNone);
			Contract.Requires(!string.IsNullOrEmpty(dbTypeName));
			Contract.Requires(getPathFunc != null);
			Contract.Requires(loadedDbs != null);

			Engine.EngineBuildHandle actual_build;
			string path = getPathFunc(forBuild, out actual_build);
			if (path == null)
				throw new InvalidOperationException(string.Format(
					"Tried to get the megalo {0} database for {1} when a build file wasn't defined for it",
					dbTypeName,
					forBuild.ToDisplayString()));

			T db;
			lock (loadedDbs)
				loadedDbs.TryGetValue(path, out db);

			if (db == null)
			{
				db = ctor(actual_build);
				lock (loadedDbs)
					loadedDbs[path] = db;

				await Task.Run(() => LoadDatabase(db, path));
			}

			return db;
		}

		public Task<MegaloStaticDatabase> GetStaticDatabaseAsync(Engine.EngineBuildHandle forBuild)
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);

			return GetDatabaseAsync(forBuild, "static",
				GetStaticDatabasePath, mLoadedStaticDbs,
				actualBuild => {
					var mdb = GetMegaloDatabaseAsync(actualBuild);
					return new MegaloStaticDatabase(actualBuild, mdb.Result.Limits);
				});
		}
		public Task<MegaloScriptDatabase> GetMegaloDatabaseAsync(Engine.EngineBuildHandle forBuild)
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);

			return GetDatabaseAsync(forBuild, "script",
				GetMegaloDatabasePath, mLoadedScriptDbs,
				actualBuild => new MegaloScriptDatabase(actualBuild));
		}

		public AllDatabasesTasksTuple GetAllDatabasesAsync(Engine.EngineBuildHandle forBuild)
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);

			var static_db_task = GetStaticDatabaseAsync(forBuild);
			var megalo_db_task = GetMegaloDatabaseAsync(forBuild);

			return new AllDatabasesTasksTuple
				( static_db_task
				, megalo_db_task
				);
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
			Contract.Requires(!gameBuild.IsNone);

			using (var system_ref = Blam.Engine.EngineRegistry.GetSystem<MegaloProtoSystem>(gameBuild))
			{
				var system = system_ref.System;
				return system.GetMegaloDatabaseAsync(gameBuild);
			}
		}
	};
}