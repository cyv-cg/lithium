using Lithium.Core.Attributes;
using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Exceptions.DefFactoryMissingException.cs
/// </summary>
public class DefFactoryMissingExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		DefFactoryMissingException ex = new DefFactoryMissingException(typeof(MockDef1));

		Assert.Equal($"{typeof(MockDef1)} has the {typeof(UseDefOverrideInitializer)} attribute but has no constructor with the {typeof(DefConstructor)} attribute or method with the {typeof(DefFactory)} attribute.", ex.Message);
	}
}
