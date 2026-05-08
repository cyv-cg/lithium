using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

public class DefInheritanceExceptionTests {
	[Fact]
	public void MessageTest01() {
		DefInheritanceException ex = new DefInheritanceException("MockDef", typeof(System.Int32), typeof(Def));

		Assert.Equal("Def 'MockDef': System.Int32 must inherit Lithium.Defs.Def.", ex.Message);
	}

	[Fact]
	public void MessageTest02() {
		DefInheritanceException ex = new DefInheritanceException("MockDef", "IntProp", typeof(System.Int32), typeof(Def));

		Assert.Equal("Def 'MockDef': property 'IntProp' is type System.Int32 which does not inherit Lithium.Defs.Def.", ex.Message);
	}
}
