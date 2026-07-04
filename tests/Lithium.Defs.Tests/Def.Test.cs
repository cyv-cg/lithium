using Xunit;
using Lithium.Strings;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Def.cs
/// </summary>
public class DefTests {
	/// <summary>
	/// Tests the setters for Def.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		Def def = new Def {
			Key = "MockDef",
			Label = new KeyedString("MockDef_Label")
		};

		Assert.Equal("MockDef", def.Key);
		Assert.Equal("MockDef_Label", def.Label.Address);
		Assert.False(def.Disabled);
	}

	/// <summary>
	/// Tests that the implicit string conversion returns the translated label.
	/// </summary>
	[Fact]
	public void ToStringTest01() {
		Def def = new Def {
			Key = "MockDef",
			Label = new KeyedString("MockDef_Label")
		};

		Assert.Equal("MockDef_Label", def);
	}
	/// <summary>
	/// Tests that ToString works with no parameters passed.
	/// </summary>
	[Fact]
	public void ToStringTest02() {
		Def def = new Def {
			Key = "MockDef",
			Label = new KeyedString("MockDef_Label")
		};

		Assert.Equal("MockDef_Label", def.ToString());
	}
	/// <summary>
	/// Tests that ToString works with parameters passed.
	/// </summary>
	[Fact]
	public void ToStringTest03() {
		Def def = new Def {
			Key = "MockDef",
			Label = new KeyedString("MockDef_Label")
		};

		Assert.Equal("MockDef_Label", def.ToString(("key", "value")));
	}

	/// <summary>
	/// Tests that two Defs are equal if they have the same key.
	/// </summary>
	[Fact]
	public void EqualsTest01() {
		Def def1 = new Def {
			Key = "DefKey",
			Label = new KeyedString("String")
		};
		Def def2 = new Def {
			Key = "DefKey",
			Label = new KeyedString("String")
		};

		Assert.True(def1.Equals(def2));
	}
	/// <summary>
	/// Tests that two Defs are unequal if they have different keys.
	/// </summary>
	[Fact]
	public void EqualsTest02() {
		Def def1 = new Def {
			Key = "DefKey",
			Label = new KeyedString("String")
		};
		Def def2 = new Def {
			Key = "AnotherDefKey",
			Label = new KeyedString("String")
		};

		Assert.False(def1.Equals(def2));
	}
	/// <summary>
	/// Tests that two Defs are unequal if the second Def is null.
	/// </summary>
	[Fact]
	public void EqualsTest03() {
		Def def1 = new Def {
			Key = "DefKey",
			Label = new KeyedString("String")
		};
		Def? def2 = null;

		Assert.False(def1.Equals(def2));
	}
}
