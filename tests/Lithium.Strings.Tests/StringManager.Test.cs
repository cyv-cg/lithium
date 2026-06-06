using System;
using System.IO;
using Fluent.Net;
using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.StringManager.cs
/// </summary>
public class StringManagerTests {
	/// <summary>
	/// Reset settings between runs.
	/// </summary>
	public StringManagerTests() {
		Settings.Reset();
	}

	/// <summary>
	/// Tests that an ResourceRootDirectoryMissingException is thrown when the StringRootDirectories setting is null while trying to reload the string contexts.
	/// </summary>
	[Fact]
	public void GetFilesInLocaleTest01() {
		Exception? ex = Assert.Throws<ResourceRootDirectoryMissingException>(
			TranslationService.Reload
		);
	}
	/// <summary>
	/// Tests that a ParseException is thrown when there is an error loading a Fluent resource file.
	/// </summary>
	[Fact]
	public void BuildContextTest01() {
		// Create a temporary file with syntax errors dynamically so the editor isn't permanently complaining about it.
		string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_ = Directory.CreateDirectory(tempDirectory);
		_ = Directory.CreateDirectory(Path.Combine(tempDirectory, "en-US"));

		string filePath = Path.Combine(tempDirectory, "en-US", "mockStrings.ftl");
		FileStream file = File.Create(filePath);
		file.Close();
		File.WriteAllText(filePath, "syntax errors!");

		try {
			Settings.AddStringRootDirectory(tempDirectory);

			Exception? ex = Assert.Throws<ParseException>(
				TranslationService.Reload
			);
		}
		finally {
			Directory.Delete(tempDirectory, true);
		}
	}
	/// <summary>
	/// Tests that strings are loaded properly from embedded resources.
	/// </summary>
	[Fact]
	public void BuildContextTest02() {
		Settings.AddEmbeddedResources(typeof(StringManagerTests).Assembly);

		TranslationService.Reload();

		Assert.Equal("This is an embedded string!", "strings.embedded-strings.test-value".Translate());
	}
	/// <summary>
	/// Tests that strings are loaded properly from embedded resources when switching locales.
	/// </summary>
	[Fact]
	public void BuildContextTest03() {
		Settings.AddEmbeddedResources(typeof(StringManagerTests).Assembly);
		Settings.SetLocale("fr-FR");

		Assert.Equal("Il s'agit d'une chaîne intégrée!", "strings.embedded-strings.test-value".Translate());
	}
}
