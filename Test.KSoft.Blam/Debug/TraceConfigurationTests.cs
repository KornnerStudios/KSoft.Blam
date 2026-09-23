using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Blam.Test;

[TestClass]
public sealed class TraceConfigurationTests
{
	[TestMethod]
	[DataRow("BlamTool.dll.config", "BlamTool", null)]
	[DataRow("MegHalomaniac.dll.config", "MegHalomaniac", "MgloGui")]
	public void AppConfig_CoversActiveTraceSources(string configFileName, string baseFileName, string? appSourceName)
	{
		string configPath = Path.Combine(AppContext.BaseDirectory, "TraceConfigs", configFileName);
		var document = XDocument.Load(configPath);
		var diagnostics = document.Root?.Element("system.diagnostics");
		Assert.IsNotNull(diagnostics);

		var configuredSources = diagnostics.Element("sources")!
			.Elements("source")
			.ToDictionary(source => (string)source.Attribute("name")!, StringComparer.Ordinal);

		var traceSources = KSoft.Debug.AssemblyTraceSourcesCollector.FromClasses(
			null,
			KSoft.Program.DebugTraceClass,
			KSoft.Blam.Program.DebugTraceClass);

		try
		{
			foreach (string sourceName in traceSources.Select(source => source.Name))
			{
				Assert.IsTrue(configuredSources.TryGetValue(sourceName, out XElement? source), $"Missing trace source '{sourceName}'.");
				AssertHasFileListener(source);
			}
		}
		finally
		{
			foreach (TraceSource traceSource in traceSources)
				traceSource.Close();
		}

		if (appSourceName != null)
		{
			Assert.IsTrue(configuredSources.TryGetValue(appSourceName, out XElement? source), $"Missing trace source '{appSourceName}'.");
			AssertHasFileListener(source);
		}

		XElement fileListener = diagnostics.Element("sharedListeners")!
			.Elements("add")
			.Single(listener => (string?)listener.Attribute("name") == "BetterTextTrace");
		Assert.AreEqual("KSoft.Debug.KSoftFileLogTraceListener, KSoft", (string?)fileListener.Attribute("type"));
		Assert.AreEqual(baseFileName, (string?)fileListener.Attribute("BaseFileName"));
	}

	private static void AssertHasFileListener(XElement? source)
	{
		Assert.IsNotNull(source);
		Assert.IsTrue(source.Element("listeners")!
			.Elements("add")
			.Any(listener => (string?)listener.Attribute("name") == "BetterTextTrace"));
	}
}
