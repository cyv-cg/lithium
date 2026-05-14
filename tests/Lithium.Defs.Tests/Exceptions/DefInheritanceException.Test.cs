using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Exceptions.DefInheritanceException.cs
/// </summary>
public class DefInheritanceExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message for bad def class inheritance.
	/// </summary>
	[Fact]
	public void MessageTest01() {
		DefInheritanceException ex = new DefInheritanceException("MockDef", typeof(int), typeof(Def));

		Assert.Equal("Def 'MockDef': System.Int32 must inherit Lithium.Defs.Def.", ex.Message);
	}
	/// <summary>
	/// Tests that the constructor creates the expected message for bad def type property inheritance.
	/// </summary>
	[Fact]
	public void MessageTest02() {
		DefInheritanceException ex = new DefInheritanceException("MockDef", "IntProp", typeof(int), typeof(Def));

		Assert.Equal("Def 'MockDef': property 'IntProp' is type System.Int32 which does not inherit Lithium.Defs.Def.", ex.Message);
	}
}
