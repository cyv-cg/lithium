using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Core.Tests;

/// <summary>
/// Tests for Lithium.Core.Exceptions.UnresolvedTypeException.cs
/// </summary>
public class UnresolvedTypeExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		UnresolvedTypeException ex = new UnresolvedTypeException("typeName");

		Assert.Equal("Could not resolve 'typeName' to a type.", ex.Message);
	}
}
