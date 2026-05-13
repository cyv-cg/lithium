using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Core.Tests;

public class ResourceRootDirectoryMissingExceptionTests {
	[Fact]
	public void ConstructorTest01() {
		ResourceRootDirectoryMissingException ex = new ResourceRootDirectoryMissingException("Resource");

		Assert.Equal("Resource root directory has not been set.", ex.Message);
	}
}
