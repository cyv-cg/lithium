using System;
using System.IO;
using Fluent.Net;
using Xunit;

namespace Lithium.Strings.Tests;

public class StringManagerTests {
	private static readonly string mocksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__mocks__");

	/// <summary>
	/// Tests that an ArgumentNullException is thrown when the StringRootDirectories setting is null while trying to reload the string contexts.
	/// </summary>
	[Fact]
	public void GetFilesInLocaleTest01() {
		Settings.Reset();
		Exception? ex = Assert.Throws<ArgumentNullException>(
			() => TranslationService.Reload()
		);
	}
	/// <summary>
	/// Tests that a ParseException is thrown when there is an error loading a Fluent resource file.
	/// </summary>
	[Fact]
	public void BuildContextTest01() {
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings02"));

		Exception? ex = Assert.Throws<ParseException>(
			() => TranslationService.Reload()
		);
	}
}
