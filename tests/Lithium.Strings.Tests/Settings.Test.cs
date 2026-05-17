using System;
using System.IO;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.Settings.cs
/// </summary>
public class SettingsTests {
	/// <summary>
	/// Tests that setting the locale updates the Locale property.
	/// </summary>
	[Fact]
	public void SetLocaleTest01() {
		Settings.Reset();
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
}
