using Xunit;
using Lithium.Defs;
using Lithium.Strings;
using System.IO;
using System;
using System.Xml;
using Lithium.Core;
using System.Reflection;
using Lithium.Core.Attributes;
using System.ComponentModel;
using Lithium.Defs.Exceptions;

namespace Lithium.Defs.Tests;

public class DefParserTests {
	public DefParserTests() {
		Init.SetupStrings();
		Settings.DeferredParsing = false;
	}

	#region LoadAll tests
	/// <summary>
	/// Tests that an Exception is thrown when the def directory has not been set.
	/// </summary>
	[Fact]
	public void LoadAllTest01() {
		Settings.SetDefRootDirectory(string.Empty);

		Assert.Throws<Exception>(
			() => DefParser.LoadAll()
		);
	}
	/// <summary>
	/// Tests that LoadAll can initialize a simple Def.
	/// </summary>
	[Fact]
	public void LoadAllTest02() {
		Init.SetupDefs(1);

		MockDef1? loadedDef = DefDatabase.Load<MockDef1>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("MockDef", loadedDef.Key);
		Assert.Equal(new KeyedString("MockDef_Label"), loadedDef.Label);
		Assert.Equal(1, loadedDef.SampleValue1);
	}
	/// <summary>
	/// Tests that LoadAll can initialize nested Defs.
	/// </summary>
	[Fact]
	public void LoadAllTest03() {
		Init.SetupDefs(2);

		MockDef2? loadedDef = DefDatabase.Load<MockDef2>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("MockDef", loadedDef.Key);
		Assert.Equal(new KeyedString("MockDef_Label"), loadedDef.Label);

		Assert.NotNull(loadedDef.SubDef);
		Assert.Equal("SecondDef", loadedDef.SubDef.Key);
		Assert.Equal(new KeyedString("MockDef_Label"), loadedDef.SubDef.Label);
		Assert.Equal(2, loadedDef.SubDef.SampleValue1);
	}

	/// <summary>
	/// Tests that LoadAll can initialize a nested list of Defs.
	/// </summary>
	[Fact]
	public void LoadAllTest04() {
		Init.SetupDefs(2);

		MockDef3? loadedDef = DefDatabase.Load<MockDef3>("ThirdDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("ThirdDef", loadedDef.Key);

		Assert.NotNull(loadedDef.DefList);
		Assert.NotEmpty(loadedDef.DefList);
		Assert.Collection<Def>(loadedDef.DefList,
			d => {
				Assert.IsType<MockDef1>(d);
				Assert.Equal("SecondDef", d.Key);
				Assert.Equal(2, ((MockDef1)d).SampleValue1);
			},
			d => {
				Assert.IsType<MockDef2>(d);
				Assert.Equal("MockDef", d.Key);
				Assert.Equal("SecondDef", ((MockDef2)d).SubDef.Key);
			}
		);
	}
	/// <summary>
	/// Tests that LoadAll throws an exception when trying to load a Def that does not exist.
	/// </summary>
	[Fact]
	public void LoadAllTest05() {
		Settings.SetDefRootDirectory(Init.MockDirectory(3));

		Assert.Throws<Exception>(
			() => DefParser.LoadAll()
		);
	}
	#endregion

	#region Load tests
	/// <summary>
	/// Tests that Load can initialize a Def with various field types, including primitives, enums, types, classes, and lists.
	/// </summary>
	[Fact]
	public void LoadTest01() {
		string mockFile = Path.Combine(Init.MockDirectory(5), "mockDefs.xml");
		DefParser.LoadSingle(mockFile);

		MockDef9? loadedDef = DefDatabase.Load<MockDef9>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal(1.2f, loadedDef.PrimitiveField);
		Assert.Equal(MockEnum.VALUE2, loadedDef.EnumField);
		Assert.Equal(typeof(System.Int32), loadedDef.TypeField);
		Assert.Equal(5, loadedDef.ClassField!.Value);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with an invalid enum value.
	/// </summary>
	[Fact]
	public void LoadTest02() {
		string mockFile = Path.Combine(Init.MockDirectory(5), "mockDefs-invalidEnum.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal($"Invalid value for enum {typeof(MockEnum)}: 'VALUE3'.", e.Message);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with an invalid type value.
	/// </summary>
	[Fact]
	public void LoadTest03() {
		string mockFile = Path.Combine(Init.MockDirectory(5), "mockDefs-invalidType.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal("Could not find type 'Type.That.Does.Not.Exist'.", e.Message);
	}
	/// <summary>
	/// Tests that Load can successfully load a Def with a type field that meets the requirements of the EnforceInheritance attribute.
	/// </summary>
	[Fact]
	public void LoadTest04() {
		string mockFile = Path.Combine(Init.MockDirectory(5), "mockDefs-inheritance-valid.xml");
		DefParser.LoadSingle(mockFile);

		MockDef10? loadedDef = DefDatabase.Load<MockDef10>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal(typeof(System.Int32), loadedDef.TypeField);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with a type that does not meet the requirements of the EnforceInheritance attribute.
	/// </summary>
	[Fact]
	public void LoadTest05() {
		string mockFile = Path.Combine(Init.MockDirectory(5), "mockDefs-inheritance-invalid.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal($"Prop 'TypeField': Type '{typeof(System.Action)}' must inherit from '{typeof(System.IComparable)}'.", e.Message);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with a property that does not exist on the Def class.
	/// </summary>
	[Fact]
	public void LoadTest06() {
		string mockFile = Path.Combine(Init.MockDirectory(5), "mockDefs-invalidProp.xml");

		Exception e = Assert.Throws<WarningException>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal($"Property 'PropThatDoesNotExist' does not exist on {typeof(MockDef1)}", e.Message);
	}
	/// <summary>
	/// Tests that Load throws a <see cref="MissingDefPropException"/> when a required field is missing.
	/// </summary>
	[Fact]
	public void LoadTest07() {
		Settings.SetDefRootDirectory(Init.MockDirectory(11));

		Exception e = Assert.Throws<MissingDefPropException>(
			() => DefParser.LoadAll()
		);
	}
	/// <summary>
	/// Tests that Load throws a <see cref="DefInheritanceException"/> when asked to load a class that does not inherit from <see cref="Def"/>.
	/// </summary>
	[Fact]
	public void LoadTest08() {
		Settings.SetDefRootDirectory(Init.MockDirectory(12));

		Exception e = Assert.Throws<DefInheritanceException>(
			() => DefParser.LoadAll()
		);
	}
	#endregion

	#region LoadFactory tests
	/// <summary>
	/// Tests that a Def can be loaded using a factory method marked with the DefFactory attribute.
	/// </summary>
	[Fact]
	public void LoadFactoryTest01() {
		string mockFile = Path.Combine(Init.MockDirectory(4), "factoryDef1.xml");
		DefParser.LoadSingle(mockFile);

		MockDef4? loadedDef = DefDatabase.Load<MockDef4>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal(15, loadedDef.FactoryClass.tenPlus);
	}
	/// <summary>
	/// Tests that a Def can be loaded using a constructor marked with the DefConstructor attribute.
	/// </summary>
	[Fact]
	public void LoadFactoryTest02() {
		string mockFile = Path.Combine(Init.MockDirectory(4), "factoryDef2.xml");
		DefParser.LoadSingle(mockFile);

		MockDef5? loadedDef = DefDatabase.Load<MockDef5>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("test", loadedDef.FactoryClass.value);
	}
	/// <summary>
	/// Tests that an exception is thrown when a class marked with the <see cref="UseOverrideDefInitializer"> attribute does not have a method with the DefFactory attribute or a constructor with the DefConstructor attribute.
	/// </summary>
	[Fact]
	public void LoadFactoryTest03() {
		string mockFile = Path.Combine(Init.MockDirectory(4), "factoryDef3.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal($"{typeof(FactoryClass3)} is marked to have an override def initializer but has no constructor with the {typeof(DefConstructor)} attribute or method with the {typeof(DefFactory)} attribute.", e.Message);
	}
	/// <summary>
	/// Tests that an exception is thrown when a factory method or constructor does not take a single parameter of type XmlNode.
	/// </summary>
	[Fact]
	public void LoadFactoryTest04() {
		string mockFile = Path.Combine(Init.MockDirectory(4), "factoryDef4.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal($"Def factory/constructor must take {typeof(XmlNode)} as the sole parameter.", e.Message);
	}
	/// <summary>
	/// Tests that an exception is thrown when a factory method or constructor returns a type that does not match the field it is being assigned to.
	/// </summary>
	[Fact]
	public void LoadFactoryTest05() {
		string mockFile = Path.Combine(Init.MockDirectory(4), "factoryDef5.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal($"Def factory must return {typeof(FactoryClass5)} but it returns {typeof(FactoryClass4)}.", e.Message);
	}
	#endregion

	#region ParseDef tests
	/// <summary>
	/// Test that ParseDef throws an exception if an invalid class name is used.
	/// </summary>
	[Fact]
	public void ParseDefTest01() {
		Settings.SetDefRootDirectory(Init.MockDirectory(6));

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadAll()
		);
		Assert.Equal("Could not find def class 'Def.Class.That.Does.Not.Exist'.", e.Message);
	}
	/// <summary>
	/// Test that ParseDef correctly applies a parent's properties regardless of load order.
	/// </summary>
	[Fact]
	public void ParseDefTest02() {
		string mockFile = Path.Combine(Init.MockDirectory(7), "mockDefs-parentValid.xml");
		DefParser.LoadSingle(mockFile);

		MockDef1? loadedDef = DefDatabase.Load<MockDef1>("MockDef");
		MockDef1? loadedDef2 = DefDatabase.Load<MockDef1>("OtherMockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal(1, loadedDef.SampleValue1);

		Assert.NotNull(loadedDef2);
		Assert.Equal(1, loadedDef2.SampleValue1);
	}
	/// <summary>
	/// Test that ParseDef throws an exception when a def tries to inherit from itself.
	/// </summary>
	[Fact]
	public void ParseDefTest03() {
		string mockFile = Path.Combine(Init.MockDirectory(7), "mockDefs-selfReference.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal("Def 'MockDef' cannot refer to itself as the root.", e.Message);
	}
	/// <summary>
	/// Test that ParseDef throws an exception when a def tries to inherit from a parent of a different class.
	/// </summary>
	[Fact]
	public void ParseDefTest04() {
		string mockFile = Path.Combine(Init.MockDirectory(7), "mockDefs-parentInvalid.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal($"Def 'MockDef' ({typeof(MockDef1)}) is attempting to inherit from 'MockParentDef' ({typeof(MockDef9)}).", e.Message);
	}
	/// <summary>
	/// Test that an exception is thrown when trying to inherit from a parent with a class that does not exist.
	/// </summary>
	[Fact]
	public void ParseDefTest05() {
		string mockFile = Path.Combine(Init.MockDirectory(7), "mockDefs-parentInvalid2.xml");

		Exception e = Assert.Throws<Exception>(
			() => DefParser.LoadSingle(mockFile)
		);
		Assert.Equal("Could not find def class 'Def.Class.That.Does.Not.Exist'.", e.Message);
	}
	#endregion
}
