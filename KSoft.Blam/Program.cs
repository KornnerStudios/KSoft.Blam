
namespace KSoft.Blam
{
	public static class Program
	{
		public static void Initialize()
		{
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