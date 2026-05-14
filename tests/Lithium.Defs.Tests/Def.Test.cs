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
}
