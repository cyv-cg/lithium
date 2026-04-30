using Xunit;
using Lithium.Core.Attributes;
using System;

namespace Lithium.Core.Tests;

public class EnforceInheritanceTests {
	[Fact]
	public void ConstructorTest01() {
		Type parentType = typeof(string);
		EnforceInheritance<string> attribute = new EnforceInheritance<string>();

		Assert.Equal(parentType, attribute.ParentType);
	}
}
