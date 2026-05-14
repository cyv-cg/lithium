using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Exceptions.DefNotFoundException.cs
/// </summary>
public class DefNotFoundExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		DefNotFoundException ex = new DefNotFoundException("MockDef");

		Assert.Equal("No Def was found with the key 'MockDef'.", ex.Message);
	}
}
