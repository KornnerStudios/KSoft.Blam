using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam
{
	[TestClass] // required for AssemblyInitialize & AssemblyCleanup to work
	static partial class TestLibrary
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			KSoft.Blam.Program.RunningUnitTests = true;

			KSoft.Program.Initialize();
			KSoft.Blam.Program.Initialize();
			Blam.Program.InitializeCoreSystems();
		}
		[AssemblyCleanup]
		public static void AssemblyDispose()
		{
			Blam.Program.DisposeCoreSystems();
			KSoft.Blam.Program.Dispose();
			KSoft.Program.Dispose();

			KSoft.Blam.Program.RunningUnitTests = false;
		}
	};

	[TestClass]
	public abstract class BaseTestClass
	{
		/// <summary>
		/// Gets or sets the test context which provides information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }
	};
}