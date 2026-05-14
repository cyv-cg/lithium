using System.Xml;
using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Exceptions.DefFactoryConstructorParamsException.cs
/// </summary>
public class DefFactoryConstructorParamsExceptionTests {
	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		DefFactoryConstructorParamsException ex = new DefFactoryConstructorParamsException(typeof(MockDef1));

		Assert.Equal($"Constructor/Factory for type Lithium.Defs.Tests.MockDef1 must take {typeof(XmlNode)} as the sole parameter.", ex.Message);
	}
}
