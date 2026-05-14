using System;
using System.IO;
using Xunit;

namespace Lithium.Defs.Tests;

public class SettingsTests {
	/// <summary>
	/// Tests that adding a valid directory to the def root directories does not throw an exception.
	/// </summary>
	[Fact]
	public void AddDefRootDirectoryTest01() {
		string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_ = Directory.CreateDirectory(tempDirectory);

		try {
			Exception? ex = Record.Exception(() => Settings.AddDefRootDirectory(tempDirectory));
			Assert.Null(ex);
		}
		finally {
			Directory.Delete(tempDirectory);
		}
	}

	/// <summary>
	/// Tests that adding a null or empty directory to the def root directories throws an ArgumentNullException.
	/// </summary>
	[Fact]
	public void AddDefRootDirectoryTest02() {
		Exception? ex = Assert.Throws<ArgumentNullException>(
			() => Settings.AddDefRootDirectory("")
		);
		Assert.NotNull(ex);
	}

	/// <summary>
	/// Tests that adding a non-existent directory to the def root directories throws a DirectoryNotFoundException.
	/// </summary>
	[Fact]
	public void AddDefRootDirectoryTest03() {
		Exception? ex = Assert.Throws<DirectoryNotFoundException>(
			() => Settings.AddDefRootDirectory("path/that/does/not/exist")
		);
		Assert.NotNull(ex);
	}
}
