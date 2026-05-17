using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Exceptions.DefFactoryReturnTypeException.cs
/// </summary>
public class DefFactoryReturnTypeExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message for an incorrect return type.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		DefFactoryReturnTypeException ex = new DefFactoryReturnTypeException(typeof(MockDef1), typeof(int));

		Assert.Equal("Def factory must return Lithium.Defs.Tests.MockDef1 but it returns System.Int32.", ex.Message);
	}
	/// <summary>
	/// Tests that the constructor creates the expected message for a null return type.
	/// </summary>
	[Fact]
	public void ConstructorTest02() {
		DefFactoryReturnTypeException ex = new DefFactoryReturnTypeException(typeof(MockDef1));

		Assert.Equal("Def factory must return Lithium.Defs.Tests.MockDef1.", ex.Message);
	}
}
