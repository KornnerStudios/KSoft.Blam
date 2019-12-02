using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Localization.Test
{
	[TestClass]
	public class LocalizationTest : BaseTestClass
	{
		[TestMethod]
		[Description("Test the core language registry for proper init and initial state")]
		public void Localization_LanguageRegistryTest()
		{
			LanguageRegistry.Initialize();

			Assert.AreEqual(17, LanguageRegistry.NumberOfLanguages,
				"Unexpected supported language count. Was a new one added?");

			Assert.IsTrue(LanguageRegistry.EnglishIndex.IsNotNone());
			Assert.AreEqual(0, LanguageRegistry.EnglishIndex,
				"Why is english not the first registered language? You know I don't speak spanish");
		}

		[TestMethod]
		// #TODO_BLAM: test should be renamed and made to validate all engines which support the LanguageSystem
		[Description("Temp test for validating the language system works using Reach")]
		public void Localization_LanguageSystemHaloReachTest()
		{
			Engine.EngineRegistry.Initialize();
			LanguageRegistry.Initialize();

			var engines = Engine.EngineRegistry.Engines;
			var reach = Engine.EngineRegistry.EngineBranchHaloReach;

			using (var locale_system = Engine.EngineRegistry.GetSystem<LanguageSystem>(reach.BranchHandle))
			{
				Assert.IsNotNull(locale_system.System);

				var lang_table = locale_system.System.GetLanguageTable(reach.BranchHandle);

				foreach (var supported_lang in lang_table.SupportedLanguageHandles)
					Console.WriteLine(supported_lang.LanguageName);
			}
		}
	};
}