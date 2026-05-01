using Xunit;

namespace Lithium.Defs.Tests;

public class DefTests {
	[Fact]
	public void ToStringTest01() {
		Init.Setup(1);

		MockDef1? loadedDef = DefDatabase.Load<MockDef1>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("label", loadedDef.ToString());
	}
}
