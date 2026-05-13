using System;
using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

public class DefFactoryReturnTypeExceptionTests {
	[Fact]
	public void ConstructorTest01() {
		DefFactoryReturnTypeException ex = new DefFactoryReturnTypeException(typeof(MockDef1), typeof(Int32));

		Assert.Equal("Def factory must return Lithium.Defs.Tests.MockDef1 but it returns System.Int32.", ex.Message);
	}

	[Fact]
	public void ConstructorTest02() {
		DefFactoryReturnTypeException ex = new DefFactoryReturnTypeException(typeof(MockDef1));

		Assert.Equal("Def factory must return Lithium.Defs.Tests.MockDef1.", ex.Message);
	}
}
