using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Core.Tests;

public class ResourceLoadFailedExceptionTests {
	[Fact]
	public void ConstructorTest01() {
		ResourceLoadFailedException ex = new ResourceLoadFailedException("Key");

		Assert.Equal("Failed to load resource 'Key'.", ex.Message);
	}
}
