using System.IO;
using Lithium.Strings.Exceptions;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.Exceptions.ResourceFormatException.cs
/// </summary>
public class ResourceFormatExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message for an external resource.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		ResourceFormatException ex = new ResourceFormatException("resource/path", false, "en-US");
		Assert.Equal(
			$"resourcePath must be in the format \'...{Path.DirectorySeparatorChar}{Path.Combine("root", "en-US", "path", "to", "resource.ext")}\': \'resource/path\'.",
			ex.Message
		);
	}
	/// <summary>
	/// Tests that the constructor creates the expected message for an embedded resource.
	/// </summary>
	[Fact]
	public void ConstructorTest02() {
		ResourceFormatException ex = new ResourceFormatException("resource/path", true, "en-US");
		Assert.Equal(
			$"resourcePath must be in the format \'root@{Path.Combine("en-US", "path", "to", "resource.ext")}\': \'resource/path\'.",
			ex.Message
		);
	}
}
