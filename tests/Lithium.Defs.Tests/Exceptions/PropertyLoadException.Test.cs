using System;
using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

public class PropertyLoadExceptionTests {
	[Fact]
	public void ConstructorTest01() {
		PropertyLoadException ex = new PropertyLoadException("MockDef", "PropName", "text", typeof(Int32));

		Assert.Equal("Def 'MockDef': could not apply value 'text' to property 'PropName' (System.Int32).", ex.Message);
	}
}
