using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Core.Tests;

public class UnresolvedTypeExceptionTests {
	[Fact]
	public void ConstructorTest01() {
		UnresolvedTypeException ex = new UnresolvedTypeException("typeName");

		Assert.Equal("Could not resolve 'typeName' to a type.", ex.Message);
	}
}
