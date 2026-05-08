using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

public class DefParentInvalidExceptionTests {
	[Fact]
	public void ConstructorTest01() {
		DefParentInvalidException ex = new DefParentInvalidException("MockDef", typeof(MockDef1), "MockDef", typeof(MockDef1));

		Assert.Equal("A def cannot be its own parent.", ex.Message);
	}

	[Fact]
	public void ConstructorTest02() {
		DefParentInvalidException ex = new DefParentInvalidException("MockDef", typeof(MockDef1), "MockParentDef", typeof(MockDef2));

		Assert.Equal($"Def 'MockParentDef' ({typeof(MockDef2)}) cannot be a parent of 'MockDef' ({typeof(MockDef1)}).", ex.Message);
	}

	[Fact]
	public void ConstructorTest03() {
		DefParentInvalidException ex = new DefParentInvalidException("MockDef", typeof(MockDef1), "MockParentDef", typeof(MockDef1));

		Assert.Equal("Parent invalid.", ex.Message);
	}
}
