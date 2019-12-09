
namespace KSoft.Blam
{
	public static class Program
	{
		public static bool RunningUnitTests = false;

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

		#region Security (temp setup)
		// #TODO_BLAM: change this setup
		static readonly byte[] kSha1Salt =
		{
			0xED, 0xD4, 0x30, 0x09, 0x66, 0x6D, 0x5C, 0x4A, 0x5C, 0x36, 0x57, 0xFA, 0xB4, 0x0E, 0x02, 0x2F,
			0x53, 0x5A, 0xC6, 0xC9, 0xEE, 0x47, 0x1F, 0x01, 0xF1, 0xA4, 0x47, 0x56, 0xB7, 0x71, 0x4F, 0x1C,
			0x36, 0xEC,
		};
		public static System.Security.Cryptography.SHA1 GetGen3RuntimeDataHasher()
		{
			var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();

			sha1.TransformBlock(kSha1Salt, 0, kSha1Salt.Length, null, 0);
			return sha1;
		}
		#endregion
	};
}