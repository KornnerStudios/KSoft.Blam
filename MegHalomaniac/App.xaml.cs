using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KSoft;

namespace MgloGui
{
	using KBlam = KSoft.Blam;
	using MegaloProto = KSoft.Blam.Megalo.Proto;

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static List<TraceSource> AllTraceSources { get; private set; } = KSoft.Debug.AssemblyTraceSourcesCollector.FromClasses(null
			, KSoft.Program.DebugTraceClass
			, KSoft.Blam.Program.DebugTraceClass
			, typeof(Debug.Trace)
			).SortAndReturn(KSoft.Debug.AssemblyTraceSourcesCollector.CompareTraceSourcesByName);

		protected override void OnStartup(StartupEventArgs e)
		{
			AppDomain.CurrentDomain.UnhandledException += new
				UnhandledExceptionEventHandler(this.AppDomainUnhandledExceptionHandler);

			base.OnStartup(e);

			KSoft.Program.Initialize();
			KSoft.Blam.Program.Initialize();
			KSoft.Blam.Program.InitializeCoreSystems();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			MgloBlamGameRequiredSystems.DisposeFromOldProgram();

			KSoft.Blam.Program.DisposeCoreSystems();
			KSoft.Blam.Program.Dispose();
			KSoft.Program.Dispose();
		}

		void AppDomainUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs ea)
		{
			var e = (Exception)ea.ExceptionObject;
			Debug.Trace.MgloGui.TraceData(TraceEventType.Error, TypeExtensions.kNone,
				"Unhandled Exception!",
				e);
		}

		#region AppIconBitmap
		static RenderTargetBitmap gAppIconBitmap;
		public static RenderTargetBitmap AppIconBitmap { get {
			// I used to have this setup in OnActivated, but that is called every time the app is put in the foreground.
			// Initializing it in OnStartup or OnLoaded is too late.
			// Lazy loading, however, works (on my machine)
			if (gAppIconBitmap == null)
				RenderAppIconBitmap();

			return gAppIconBitmap;
		} }

		private static void RenderAppIconBitmap()
		{
			var grid = (Grid)Application.Current.FindResource("MgloLogoGrid");
			if (grid == null)
				throw new ArgumentException("Failed to find logo Grid", "MgloLogoGrid");

			var viewbox = new Viewbox();
			viewbox.Child = grid;
			viewbox.Measure(new Size(512, 512));
			viewbox.Arrange(new Rect(0, 0, 512, 512));
			viewbox.UpdateLayout();

			var viewbox_ps = PresentationSource.FromVisual(viewbox);
			double dpiX = 96.0, dpiY = 96.0;
			if (viewbox_ps != null)
			{
				dpiX *= viewbox_ps.CompositionTarget.TransformToDevice.M11;
				dpiY *= viewbox_ps.CompositionTarget.TransformToDevice.M22;
			}

			gAppIconBitmap = new RenderTargetBitmap((int)viewbox.ActualWidth, (int)viewbox.ActualHeight, dpiX, dpiY, PixelFormats.Pbgra32);
			gAppIconBitmap.Render(viewbox);
		}
		#endregion
	};

	public class MgloBlamGameRequiredSystems : IDisposable
	{
		public KBlam.Engine.BlamEngineTargetHandle GameBuildAndTarget = KBlam.Engine.BlamEngineTargetHandle.None;
		public KBlam.Engine.EngineSystemReference<KBlam.Blob.BlobSystem> BlobSystemRef = KBlam.Engine.EngineSystemReference<KBlam.Blob.BlobSystem>.None;
		public KBlam.Engine.EngineSystemReference<KBlam.Localization.LanguageSystem> LangSystemRef = KBlam.Engine.EngineSystemReference<KBlam.Localization.LanguageSystem>.None;
		public KBlam.Engine.EngineSystemReference<MegaloProto.MegaloProtoSystem> MegaloSystemRef = KBlam.Engine.EngineSystemReference<MegaloProto.MegaloProtoSystem>.None;
		private bool mIsLoaded;

		public MgloBlamGameRequiredSystems(KBlam.Engine.BlamEngineTargetHandle gameBuildAndTarget)
		{
			GameBuildAndTarget = gameBuildAndTarget;
		}

		public void Dispose()
		{
			MegaloSystemRef.Dispose();
			LangSystemRef.Dispose();
			BlobSystemRef.Dispose();
			mIsLoaded = false;
		}

		public void LoadSystems()
		{
			if (mIsLoaded)
				return;

			BlobSystemRef = KBlam.Engine.EngineRegistry.GetSystem<KBlam.Blob.BlobSystem>(GameBuildAndTarget.Build);
			LangSystemRef = KBlam.Engine.EngineRegistry.GetSystem<KBlam.Localization.LanguageSystem>(GameBuildAndTarget.Build);
			LoadMegaloSystem();

			mIsLoaded = true;
		}

		void LoadMegaloSystem()
		{
			MegaloSystemRef = KBlam.Engine.EngineRegistry.GetSystem<MegaloProto.MegaloProtoSystem>(GameBuildAndTarget.Build);

			var megalo_proto_system = MegaloSystemRef.System;
			var all_dbs_tasks = megalo_proto_system.GetAllDatabasesAsync(GameBuildAndTarget.Build);

			Exception inner_exception = null;
			if (all_dbs_tasks.Item1.IsFaulted)
			{
				inner_exception = all_dbs_tasks.Item1.Exception.GetOnlyExceptionOrAll();
			}
			else if (all_dbs_tasks.Item2.IsFaulted)
			{
				inner_exception = all_dbs_tasks.Item2.Exception.GetOnlyExceptionOrAll();
			}

			if (inner_exception != null)
			{
				throw new InvalidOperationException("Megalo is not supported for this version or no DB exists for it: " +
					GameBuildAndTarget.Build.ToDisplayString(),
					inner_exception);
			}

			Task.WaitAll
				( all_dbs_tasks.Item1
				, all_dbs_tasks.Item2
				);

			megalo_proto_system.PrepareDatabasesForUse(all_dbs_tasks.Item1.Result, all_dbs_tasks.Item2.Result);
		}

		static Dictionary<string, MgloBlamGameRequiredSystems> gCachedGames = new Dictionary<string, MgloBlamGameRequiredSystems>();
		public static MgloBlamGameRequiredSystems GetOrCreateFromSelectableGameBuildName(string name)
		{
			MgloBlamGameRequiredSystems instance;
			lock (gCachedGames)
			{
				if (!gCachedGames.TryGetValue(name, out instance))
				{
					KBlam.Engine.EngineBuildRevision build_revision = KBlam.Engine.EngineRegistry.TryParseExportedBuildName(name);
					if (build_revision == null)
						throw new ArgumentOutOfRangeException(nameof(name), name, "Not a valid build name");

					instance = new MgloBlamGameRequiredSystems(build_revision.BuildHandle.ToEngineTargetHandle());
					gCachedGames.Add(name, instance);
				}
			}

			return instance;
		}

		public static void DisposeFromOldProgram()
		{
			foreach (var kvp in gCachedGames)
			{
				kvp.Value.Dispose();
			}
		}
	};
}

namespace MgloGui.Properties
{
	partial class Settings
	{
		private static List<string> mSelectableGameBuildNameValuesSupportingMegalo = new List<string>();
		public IReadOnlyList<string> SelectableGameBuildNameValuesSupportingMegalo { get {
			if (mSelectableGameBuildNameValuesSupportingMegalo.Count == 0 &&
				KSoft.Blam.Engine.EngineRegistry.IsInitialized)
			{
				foreach (var kvp in KSoft.Blam.Engine.EngineRegistry.ExportedBuildsByName)
				{
					var specific_build_handle = kvp.Value.BuildHandle;
					using (var system_ref = KSoft.Blam.Engine.EngineRegistry.TryGetSystem<KSoft.Blam.Megalo.Proto.MegaloProtoSystem>(specific_build_handle))
					{
						if (system_ref.IsValid && system_ref.System.IsSpecificBuildSupported(specific_build_handle))
						{
							mSelectableGameBuildNameValuesSupportingMegalo.Add(kvp.Key);
						}
					}
				}
			}

			return mSelectableGameBuildNameValuesSupportingMegalo;
		} }
	};
}
