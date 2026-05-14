using Xunit;
using Lithium.Strings;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.Def.cs
/// </summary>
public class DefTests {
	/// <summary>
	/// Tests the setters for Def.
	/// Records are funny, so this has to be done after instantiating for full test coverage.
	/// https://stackoverflow.com/questions/70455702/how-to-get-code-coverage-on-c-sharp-record-setters-with-positional-constructor
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		Def def = new Def {
			Key = "MockDef",
			Label = (KeyedString)"MockDef_Label"
		};

		def = def with {
			Key = "MockDef",
			Label = (KeyedString)"MockDef_Label"
		};

		Assert.Equal("MockDef", def.Key);
		Assert.Equal("MockDef_Label", def.Label.Address);
	}

	/// <summary>
	/// Tests that the implicit string conversion returns the translated label.
	/// </summary>
	[Fact]
	public void ToStringTest01() {
		Def def = new Def {
			Key = "MockDef",
			Label = (KeyedString)"MockDef_Label"
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
			Label = (KeyedString)"MockDef_Label"
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
			Label = (KeyedString)"MockDef_Label"
		};

		Assert.Equal("MockDef_Label", def.ToString(("key", "value")));
	}
}
