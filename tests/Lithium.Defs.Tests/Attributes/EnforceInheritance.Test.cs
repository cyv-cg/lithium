using Xunit;
using System;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.EnforceInheritance.cs
/// </summary>
public class EnforceInheritanceTests {
	/// <summary>
	/// Tests that the constructor stores the type parameter into a property.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		Type parentType = typeof(string);
		EnforceInheritance<string> attribute = new EnforceInheritance<string>();

		Assert.Equal(parentType, attribute.ParentType);
	}
}
