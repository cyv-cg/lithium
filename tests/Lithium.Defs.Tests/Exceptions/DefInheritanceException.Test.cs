using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

public class DefInheritanceExceptionTests {
	[Fact]
	public void MessageTest01() {
		DefInheritanceException ex = new DefInheritanceException(typeof(System.Int32));

		Assert.Equal("System.Int32 must inherit Lithium.Defs.Def", ex.Message);
	}
}
