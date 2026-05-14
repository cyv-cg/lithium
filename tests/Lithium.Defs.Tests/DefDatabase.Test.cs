using System;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

public class DefDatabaseTests {
	public DefDatabaseTests() {
		Init.SetupStrings();
		Settings.DeferredParsing = false;
	}

	/// <summary>
	/// Tests that the DefDatabase is properly initialized and that no defs are loaded when the files contain no defs.
	/// </summary>
	[Fact]
	public void InitializeTest01() {
		Init.SetupDefs(8);

		Assert.Empty(DefDatabase.LoadAll());
	}

	/// <summary>
	/// Tests that GetDefKey throws an exception when the provided XML node does not contain a 'key' child element.
	/// </summary>
	[Fact]
	public void GetDefKeyTest01() {
		Exception e = Assert.Throws<NodeMissingChildException>(
			() => Init.SetupDefs(9)
		);
	}

	/// <summary>
	/// Tests that LoadXml throws an exception when no def with the provided key exists in the database.
	/// </summary>
	[Fact]
	public void LoadXmlTest01() {
		Exception e = Assert.Throws<DefNotFoundException>(
			() => Init.SetupDefs(10)
		);
	}

	/// <summary>
	/// Tests that Load returns null when no def with the provided key exists in the database.
	/// </summary>
	[Fact]
	public void LoadTest01() {
		Init.SetupDefs(1);

		Def? loadedDef = DefDatabase.Load<MockDef5>("MockDef");
		Assert.Null(loadedDef);
	}

	/// <summary>
	/// Tests that Load returns the def properly with deferred loading.
	/// </summary>
	[Fact]
	public void LoadTest02() {
		Settings.SetDefRootDirectory(Init.MockDirectory(1));
		Settings.DeferredParsing = true;
		DefParser.LoadAll();

		MockDef1? loadedDef = DefDatabase.Load<MockDef1>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("MockDef", loadedDef.Key);
		Assert.Equal("MockDef_Label", loadedDef.Label.key);
		Assert.Equal(1, loadedDef.SampleValue1);
	}

	/// <summary>
	/// Tests that Load returns null when given a key that does not exist with deferred loading.
	/// </summary>
	[Fact]
	public void LoadTest03() {
		Settings.SetDefRootDirectory(Init.MockDirectory(1));
		Settings.DeferredParsing = true;
		DefParser.LoadAll();

		MockDef1? loadedDef = DefDatabase.Load<MockDef1>("MockDefThatDoesNotExist");

		Assert.Null(loadedDef);
	}

	/// <summary>
	/// Tests that Load returns null when the provided type does not match the actual type with deferred loading.
	/// </summary>
	[Fact]
	public void LoadTest04() {
		Settings.SetDefRootDirectory(Init.MockDirectory(1));
		Settings.DeferredParsing = true;
		DefParser.LoadAll();

		MockDef2? loadedDef = DefDatabase.Load<MockDef2>("MockDef");

		Assert.Null(loadedDef);
	}
}
