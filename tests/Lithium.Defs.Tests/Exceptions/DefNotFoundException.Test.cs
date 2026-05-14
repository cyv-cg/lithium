using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

public class DefNotFoundExceptionTests {
	[Fact]
	public void ConstructorTest01() {
		DefNotFoundException ex = new DefNotFoundException("MockDef");

		Assert.Equal("No Def was found with the key 'MockDef'.", ex.Message);
	}
}
