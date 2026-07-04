using System;
using Xunit;
using Lithium.Defs.Utils;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.DefUtils.cs
/// </summary>
public class DefUtilsTests {
	#region CopyTo
	/// <summary>
	/// Tests that CopyTo copies all property values to the target.
	/// </summary>
	[Fact]
	public void CopyToTest01() {
		MockDef1 A = new MockDef1 {
			Key = "DefKey",
			Label = new Strings.KeyedString("Label"),
			SampleValue1 = 5
		};
		Def B = new MockDef1 {
			Key = "DefKey^",
			Label = new Strings.KeyedString("DefKey^")
		};

		A.CopyTo(ref B);

		Assert.Equal(A.Key, B.Key);
		Assert.Equal(A.Label.Address, B.Label.Address);
		Assert.Equal(A.SampleValue1, ((MockDef1)B).SampleValue1);
	}
	/// <summary>
	/// Tests that CopyTo throws an ArgumentException if the target Def's type does not match the source.
	/// </summary>
	[Fact]
	public void CopyToTest02() {
		MockDef1 A = new MockDef1 {
			Key = "DefKey",
			Label = new Strings.KeyedString("Label"),
			SampleValue1 = 5
		};
		Def B = new MockDef11 {
			Key = "DefKey^",
			Label = new Strings.KeyedString("DefKey^")
		};

		Exception ex = Assert.Throws<ArgumentException>(
			() => A.CopyTo(ref B)
		);
		Assert.NotNull(ex);
	}
	#endregion
}
