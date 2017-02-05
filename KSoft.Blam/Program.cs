using System;

namespace KSoft.Blam
{
	public static class Program
	{
		public static void Initialize()
		{
			Util.ValueTypeInitializeComparer			<Engine.EngineBuildHandle>();
			Util.ValueTypeInitializeEquatableComparer	<Engine.EngineBuildHandle>();
			Util.ValueTypeInitializeComparer			<Engine.BlamEngineTargetHandle>();
			Util.ValueTypeInitializeEquatableComparer	<Engine.BlamEngineTargetHandle>();
			Util.ValueTypeInitializeComparer			<Localization.GameLanguageHandle>();
			Util.ValueTypeInitializeEquatableComparer	<Localization.GameLanguageHandle>();

			Engine.EngineRegistry.InitializeForNewProgram();
		}
		public static void Dispose()
		{
			Engine.EngineRegistry.DisposeFromOldProgram();
		}

		public static void InitializeCoreSystems()
		{
			Localization.LanguageRegistry.Initialize();
			Engine.EngineRegistry.Initialize();
		}
		public static void DisposeCoreSystems()
		{
			Engine.EngineRegistry.Dispose();
			Localization.LanguageRegistry.Dispose();
		}
	};
}