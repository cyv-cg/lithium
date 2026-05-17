using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Core.Exceptions.PropertyLoadException.cs
/// </summary>
public class PropertyLoadExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		PropertyLoadException ex = new PropertyLoadException("MockDef", "PropName", "text", typeof(int));

		Assert.Equal("Def 'MockDef': could not apply value 'text' to property 'PropName' (System.Int32).", ex.Message);
	}
}
