using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Core.Tests;

/// <summary>
/// Tests for Lithium.Core.Exceptions.ResourceLoadFailedException.cs
/// </summary>
public class ResourceLoadFailedExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		ResourceLoadFailedException ex = new ResourceLoadFailedException("Key");

		Assert.Equal("Failed to load resource 'Key'.", ex.Message);
	}
}
