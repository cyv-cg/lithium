using System;
using Xunit;

namespace Lithium.Defs.Tests;

public class DefDatabaseTests {
	/// <summary>
	/// Tests that the DefDatabase is properly initialized and that no defs are loaded when the files contain no defs.
	/// </summary>
	[Fact]
	public void InitializeTest01() {
		Init.Setup(8);

		Assert.Empty(DefDatabase.LoadAll());
	}

	/// <summary>
	/// Tests that GetDefKey throws an exception when the provided XML node does not contain a 'key' child element.
	/// </summary>
	[Fact]
	public void GetDefKeyTest01() {
		Exception e = Assert.Throws<Exception>(
			() => Init.Setup(9)
		);
		Assert.Equal("Def node missing 'key' child element.", e.Message);
	}

	/// <summary>
	/// Tests that LoadXml throws an exception when no def with the provided key exists in the database.
	/// </summary>
	[Fact]
	public void LoadXmlTest01() {
		Exception e = Assert.Throws<Exception>(
			() => Init.Setup(10)
		);
		Assert.Equal("No Def was found with the key 'DefThatDoesNotExist'.", e.Message);
	}

	/// <summary>
	/// Tests that Load returns null when no def with the provided key exists in the database.
	/// </summary>
	[Fact]
	public void LoadTest01() {
		Init.Setup(1);

		Def? loadedDef = DefDatabase.Load<MockDef5>("MockDef");
		Assert.Null(loadedDef);
	}
}
