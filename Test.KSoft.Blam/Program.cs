using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam
{
	[TestClass] // required for AssemblyInitialize & AssemblyCleanup to work
	static partial class TestLibrary
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			KSoft.Program.Initialize();
			KSoft.Blam.Program.Initialize();

			// If this isn't true, then whoever is using this code didn't update
			// [kTestResultsPath] to reflect their project tree's test results dir.
			//
			// We don't use the TextContext's TestDir properties because there have
			// been issues with VS using the "In" dir, and deleting whatever gets
			// outputted into there. But hey, maybe I'm just doing something wrong!
			Assert.IsTrue(System.IO.Directory.Exists(BaseTestClass.kTestResultsPath));
		}
		[AssemblyCleanup]
		public static void AssemblyDispose()
		{
			KSoft.Blam.Program.Dispose();
			KSoft.Program.Dispose();
		}
	};

	[TestClass]
	public abstract class BaseTestClass
	{
		public const string kTestResultsPath = @"C:\Mount\B\Kornner\Vita\_test_results\KSoft.Blam\";

		/// <summary>
		/// Gets or sets the test context which provides information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }
	};
}