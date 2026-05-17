using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Core.Tests;

/// <summary>
/// Tests for Lithium.Core.Exceptions.ResourceRootDirectoryMissingException.cs
/// </summary>
public class ResourceRootDirectoryMissingExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		ResourceRootDirectoryMissingException ex = new ResourceRootDirectoryMissingException("Resource");

		Assert.Equal("Resource root directory has not been set.", ex.Message);
	}
}
