using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Localization.Test
{
	[TestClass]
	public class LocalizationTest : BaseTestClass
	{
		[TestMethod]
		public void Localization_LanguageSystemTest()
		{
			LanguageRegistry.Initialize();

			Assert.AreEqual(17, LanguageRegistry.NumberOfLanguages,
				"Unexpected supported language count. Was a new one added?");

			Assert.IsTrue(LanguageRegistry.EnglishIndex.IsNotNone());
			Assert.AreEqual(0, LanguageRegistry.EnglishIndex,
				"Why is english not the first registered language? You know I don't speak spanish");
		}
	};
}