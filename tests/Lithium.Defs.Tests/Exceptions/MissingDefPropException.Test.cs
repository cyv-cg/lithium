using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Exceptions.MissingDefPropException.cs
/// </summary>
public class MissingDefPropExceptionTests {
	/// <summary>
	/// Tests that MissingDefPropException properly enumerates the names of missing properties.
	/// </summary>
	[Fact]
	public void MessageTest01() {
		MissingDefPropException ex = new MissingDefPropException("MockDef", null, typeof(MockDef1).GetProperties());

		Assert.Equal(
			"Missing fields in def 'MockDef': SampleValue1, Key, Label, Disabled",
			ex.Message
		);
	}
	/// <summary>
	/// Tests that MissingDefPropException properly formats a single missing property.
	/// </summary>
	[Fact]
	public void MessageTest02() {
		MissingDefPropException ex = new MissingDefPropException("MockDef", null, typeof(MockDataClass).GetProperties());

		Assert.Equal(
			"Missing fields in def 'MockDef': Value",
			ex.Message
		);
	}
	/// <summary>
	/// Tests that MissingDefPropException properly formats properties within a Def.
	/// </summary>
	[Fact]
	public void MessageTest03() {
		MissingDefPropException ex = new MissingDefPropException("MockDef", "PropertyName", typeof(MockDataClass).GetProperties());

		Assert.Equal(
			"Missing fields in def 'MockDef' property 'PropertyName': Value",
			ex.Message
		);
	}
}
