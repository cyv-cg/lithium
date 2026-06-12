using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.Settings.cs
/// </summary>
public class SettingsTests {
	/// <summary>
	/// Reset state between runs.
	/// </summary>
	public SettingsTests() {
		Settings.Reset();
	}

	/// <summary>
	/// Tests that setting the locale updates the Locale property.
	/// </summary>
	[Fact]
	public void SetLocaleTest01() {
		Settings.SetLocale("en-US");
		Assert.Equal("English (United States)", Settings.Locale.DisplayName);
	}

	/// <summary>
	/// Tests that adding a valid directory to the string root directories does not throw an exception.
	/// </summary>
	[Fact]
	public void AddStringRootDirectoryTest01() {
		string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_ = Directory.CreateDirectory(tempDirectory);

		try {
			Exception? ex = Record.Exception(() => Settings.AddStringRootDirectory(tempDirectory));
			Assert.Null(ex);
		}
		finally {
			Directory.Delete(tempDirectory);
		}
	}

	/// <summary>
	/// Tests that adding a null or empty directory to the string root directories throws an ArgumentNullException.
	/// </summary>
	[Fact]
	public void AddStringRootDirectoryTest02() {
		Exception? ex = Assert.Throws<ArgumentNullException>(
			() => Settings.AddStringRootDirectory("")
		);
		Assert.NotNull(ex);
	}

	/// <summary>
	/// Tests that adding a non-existent directory to the string root directories throws a DirectoryNotFoundException.
	/// </summary>
	[Fact]
	public void AddStringRootDirectoryTest03() {
		Exception? ex = Assert.Throws<DirectoryNotFoundException>(
			() => Settings.AddStringRootDirectory("path/that/does/not/exist")
		);
		Assert.NotNull(ex);
	}

	/// <summary>
	/// Tests that adding an assembly adds all its strings.
	/// </summary>
	[Fact]
	public void AddEmbeddedResourcesTest01() {
		Settings.AddEmbeddedResources(typeof(SettingsTests).Assembly);
		IEnumerable<string> resources = TranslationService.GetAllLoadedStrings();

		string s = Assert.Single(resources);
		Assert.Equal("strings.embedded-strings.test-value", s);
	}

	/// <summary>
	/// Tests that adding an assembly twice throws an exception.
	/// </summary>
	[Fact]
	public void AddEmbeddedResourcesTest02() {
		Settings.AddEmbeddedResources(typeof(SettingsTests).Assembly);
		Exception? ex = Assert.Throws<ArgumentException>(
			() => Settings.AddEmbeddedResources(typeof(SettingsTests).Assembly)
		);
		Assert.NotNull(ex);
	}

	/// <summary>
	/// Tests that adding an assembly with no strings results in no strings being added.
	/// </summary>
	[Fact]
	public void AddEmbeddedResourcesTest03() {
		Settings.AddEmbeddedResources(typeof(Settings).Assembly);
		Assert.Empty(TranslationService.GetAllLoadedStrings());
	}
}
