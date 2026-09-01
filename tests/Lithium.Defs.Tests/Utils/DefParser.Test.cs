using Xunit;
using Lithium.Strings;
using System.IO;
using System;
using Lithium.Defs.Exceptions;
using Lithium.Core.Exceptions;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using Lithium.Core;
using Lithium.Defs.XML;
using System.Text;
using System.Reflection;
using System.Runtime.Loader;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.DefParser.cs
/// </summary>
public class DefParserTests {
	private readonly DefService service;
	private static IEnumerable<Assembly>? baseAssemblies;

	/// <summary>
	/// Reset the state for each test.
	/// </summary>
	public DefParserTests() {
		service = new DefService(new DefServiceOptions());
		baseAssemblies ??= AppDomain.CurrentDomain.GetAssemblies();
		TypeChecker.assemblyScraper = new AssemblyScraper(baseAssemblies);
	}

	#region ParseDef tests
	/// <summary>
	/// Test that ParseDef throws an exception if an invalid class name is used.
	/// </summary>
	[Fact]
	public void ParseDefTest01() {
		Init.Setup(6, service);

		Exception e = Assert.Throws<UnresolvedTypeException>(
			() => service.LoadDef<Def>("MockDef")
		);
	}
	/// <summary>
	/// Test that ParseDef correctly applies a parent's properties regardless of load order.
	/// </summary>
	[Fact]
	public void ParseDefTest02() {
		string mockFile = Path.Combine(Init.MockDirectory(7), "parent-valid");
		_ = service.RegisterResource(mockFile, out _);
		service.Reload();

		MockDef1? loadedDef = service.LoadDef<MockDef1>("MockDef");
		MockDef1? loadedDef2 = service.LoadDef<MockDef1>("OtherMockDef");

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
		string mockFile = Path.Combine(Init.MockDirectory(7), "self-reference");
		_ = service.RegisterResource(mockFile, out _);
		service.Reload();

		Exception e = Assert.Throws<DefParentInvalidException>(
			() => service.LoadDef<Def>("MockDef")
		);
	}
	/// <summary>
	/// Test that ParseDef throws an exception when a def tries to inherit from a parent of a different class.
	/// </summary>
	[Fact]
	public void ParseDefTest04() {
		string mockFile = Path.Combine(Init.MockDirectory(7), "parent-invalid");
		_ = service.RegisterResource(mockFile, out _);
		service.Reload();

		Exception e = Assert.Throws<DefParentInvalidException>(
			() => service.LoadDef<Def>("MockDef")
		);
	}
	/// <summary>
	/// Test that an exception is thrown when trying to inherit from a parent with a class that does not exist.
	/// </summary>
	[Fact]
	public void ParseDefTest05() {
		string mockFile = Path.Combine(Init.MockDirectory(7), "parent-invalid-2");
		_ = service.RegisterResource(mockFile, out _);
		service.Reload();

		Exception e = Assert.Throws<UnresolvedTypeException>(
			() => service.LoadDef<Def>("MockDef")
		);
	}
	/// <summary>
	/// Test that ParseDef can parse objects in various valid configurations.
	/// </summary>
	[Fact]
	public void ParseDefTest06() {
		_ = service.RegisterResource(Init.MockDirectory(15), out StringBuilder? errors);
		service.Reload();

		IEnumerable<Def> defs = service.LoadAll();

		Assert.Null(errors);
		Assert.Collection(defs.OrderBy(d => d.Key),
			d => {
				MockDef14 def = (d as MockDef14)!;

				Assert.Equal("DefA", d.Key);
				Assert.Equal("DefB", def.Pointer.Key);
			},
			d => {
				MockDef14 def = (d as MockDef14)!;

				Assert.Equal("DefB", d.Key);
				Assert.Equal("DefC", def.Pointer.Key);
			},
			d => {
				MockDef14 def = (d as MockDef14)!;

				Assert.Equal("DefC", d.Key);
				Assert.Equal("DefD", def.Pointer.Key);
			},
			d => {
				MockDef14 def = (d as MockDef14)!;

				Assert.Equal("DefD", d.Key);
				Assert.Equal("DefA", def.Pointer.Key);
			},
			d => {
				MockDef4 def = (d as MockDef4)!;

				Assert.Equal("FactoryDef", d.Key);
				Assert.Equal("MockDef_Label", d.Label.Address);
				Assert.Equal(11, def.FactoryClass.tenPlus);
			},
			d => {
				MockDef12 def = (d as MockDef12)!;

				Assert.Equal("MasterDef", d.Key);
				Assert.Equal("MockDef_Label", d.Label.Address);
				Assert.Equal(0.1f, def.PrimitiveField);
				Assert.Equal(MockEnum.VALUE1, def.EnumField);
				Assert.Equal(typeof(MockDataClass), def.TypeField);
				Assert.Equal(40, def.ClassField!.Value);
				Assert.Equal(new List<int> { 1, 2, 3 }, def.ListField);
				Assert.Collection(def.DefList.OrderBy(e => e.Key),
					s => {
						Assert.Equal("FactoryDef", s.Key);
					},
					s => {
						Assert.Equal("MockDef-Self-Reference", s.Key);
					},
					s => {
						Assert.Equal("MockDef2", s.Key);
					},
					s => {
						Assert.Equal("MockDef3", s.Key);
					}
				);
			},
			d => {
				MockDef15 def = (d as MockDef15)!;

				Assert.Equal("MockDef-ClassList", d.Key);

				Assert.NotNull(def.DataSingle.Value);
				Assert.Equal("MockDef2", def.DataSingle.Value.Key);

				MockDefDataClass @class = Assert.Single(def.DataList);
				Assert.NotNull(@class.Value);
				Assert.Equal("FactoryDef", @class.Value.Key);
			},
			d => {
				MockDef9 def = (d as MockDef9)!;

				Assert.Equal("MockDef-EmptyList", d.Key);
				Assert.NotNull(def.ListField);
				Assert.Empty(def.ListField);
			},
			d => {
				MockDef13 def = (d as MockDef13)!;

				Assert.Equal("MockDef-Self-Reference", d.Key);
				Assert.Equal("MockDef_Label", d.Label.Address);
				Assert.Equal("MockDef-Self-Reference", def.NestedDef1.Key);
				Assert.Equal("MasterDef", def.NestedDef2.Key);
				Assert.Equal("MockDef3", def.NestedDef3.Key);
				Assert.Equal("MasterDef", (def.NestedDef3 as MockDef3)!.DefList.First().Key);
			},
			d => {
				MockDef1 def = (d as MockDef1)!;

				Assert.Equal("MockDef-Simple", d.Key);
				Assert.Equal("MockDef_Label", d.Label.Address);
				Assert.Equal(3, def.SampleValue1);
			},
			d => {
				MockDef2 def = (d as MockDef2)!;

				Assert.Equal("MockDef2", d.Key);
				Assert.Equal("MockDef_Label", d.Label.Address);
				Assert.Equal("MockDef-Simple", def.SubDef.Key);
			},
			d => {
				MockDef2 def = (d as MockDef2)!;

				Assert.Equal("MockDef2-Parent", d.Key);
				Assert.Equal("MockDef_Label", d.Label.Address);
				Assert.Equal("MockDef-Simple", def.SubDef.Key);
			},
			d => {
				MockDef3 def = (d as MockDef3)!;

				Assert.Equal("MockDef3", d.Key);
				Assert.Equal("MockDef_Label", d.Label.Address);
				_ = Assert.Single(def.DefList);
				Assert.Equal("MasterDef", def.DefList.First().Key);
			},
			d => {
				MockDef12 def = (d as MockDef12)!;

				Assert.Equal("MockDefChild9", def.Key);
				Assert.Equal(25, def.PrimitiveField);
				Assert.Empty(def.DefList);
			},
			d => {
				MockDef9 def = (d as MockDef9)!;

				Assert.Equal("MockDefParent9", def.Key);
				Assert.Equal(25, def.PrimitiveField);
			}
		);
	}
	/// <summary>
	/// Tests that ParseDef fails under various conditions
	/// </summary>
	[Fact]
	public void ParseDefTest07() {
		XmlNodeList nodes = XmlLoader.LoadDocument(Path.Combine(Init.MockDirectory(16), "mockDefs.xml"))!.FirstChild!.ChildNodes!;

		Exception ex1 = Assert.Throws<UnresolvedTypeException>(
			() => DefParser.ParseDef(service, nodes[0]!)
		);
		Exception ex2 = Assert.Throws<DefInheritanceException>(
			() => DefParser.ParseDef(service, nodes[1]!)
		);
		Exception ex3 = Assert.Throws<DefNotFoundException>(
			() => DefParser.ParseDef(service, nodes[2]!)
		);
		Exception ex4 = Assert.Throws<DefNotFoundException>(
			() => DefParser.ParseDef(service, nodes[3]!)
		);

		Assert.NotNull(ex1);
		Assert.NotNull(ex2);
		Assert.NotNull(ex3);
		Assert.NotNull(ex4);
	}

	/// <summary>
	/// Tests that LoadAll can initialize a simple Def.
	/// </summary>
	[Fact]
	public void ParseDefTest08() {
		Init.Setup(1, service);

		MockDef1? loadedDef = service.LoadDef<MockDef1>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("MockDef", loadedDef.Key);
		Assert.True(loadedDef.Label.Equals("MockDef_Label"));
		Assert.Equal(1, loadedDef.SampleValue1);
	}
	/// <summary>
	/// Tests that LoadAll can initialize nested Defs.
	/// </summary>
	[Fact]
	public void ParseDefTest09() {
		Init.Setup(2, service);

		MockDef2? loadedDef = service.LoadDef<MockDef2>("FirstDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("FirstDef", loadedDef.Key);
		Assert.True(loadedDef.Label.Equals("MockDef_Label"));

		Assert.NotNull(loadedDef.SubDef);
		Assert.Equal("SecondDef", loadedDef.SubDef.Key);
		Assert.True(loadedDef.SubDef.Label.Equals("MockDef_Label"));
		Assert.Equal(2, loadedDef.SubDef.SampleValue1);
	}

	/// <summary>
	/// Tests that LoadAll can initialize a nested list of Defs.
	/// </summary>
	[Fact]
	public void ParseDefTest10() {
		Init.Setup(2, service);

		MockDef3? loadedDef = service.LoadDef<MockDef3>("ThirdDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("ThirdDef", loadedDef.Key);

		Assert.NotNull(loadedDef.DefList);
		Assert.NotEmpty(loadedDef.DefList);
		Assert.Collection(loadedDef.DefList.OrderBy(d => d.Key),
			d => {
				_ = Assert.IsType<MockDef2>(d);
				Assert.Equal("FirstDef", d.Key);
				Assert.Equal("SecondDef", ((MockDef2)d).SubDef.Key);
			},
			d => {
				_ = Assert.IsType<MockDef1>(d);
				Assert.Equal("SecondDef", d.Key);
				Assert.Equal(2, ((MockDef1)d).SampleValue1);
			}
		);
	}
	/// <summary>
	/// Tests that LoadAll can initialize a simple Def with deferred loading.
	/// </summary>
	[Fact]
	public void ParseDefTest11() {
		Init.Setup(1, service);

		MockDef1? loadedDef = service.LoadDef<MockDef1>("MockDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("MockDef", loadedDef.Key);
		Assert.Equal((KeyedString)"MockDef_Label", loadedDef.Label);
		Assert.Equal(1, loadedDef.SampleValue1);
	}

	/// <summary>
	/// Tests that LoadAll can initialize nested Defs with deferred loading.
	/// </summary>
	[Fact]
	public void ParseDefTest12() {
		Init.Setup(2, service);

		MockDef2? loadedDef = service.LoadDef<MockDef2>("FirstDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("FirstDef", loadedDef.Key);
		Assert.Equal((KeyedString)"MockDef_Label", loadedDef.Label);

		Assert.NotNull(loadedDef.SubDef);
		Assert.Equal("SecondDef", loadedDef.SubDef.Key);
		Assert.Equal((KeyedString)"MockDef_Label", loadedDef.SubDef.Label);
		Assert.Equal(2, loadedDef.SubDef.SampleValue1);
	}

	/// <summary>
	/// Tests that LoadAll can initialize a nested list of Defs with deferred loading.
	/// </summary>
	[Fact]
	public void ParseDefTest13() {
		Init.Setup(2, service);

		MockDef3? loadedDef = service.LoadDef<MockDef3>("ThirdDef");

		Assert.NotNull(loadedDef);
		Assert.Equal("ThirdDef", loadedDef.Key);

		Assert.NotNull(loadedDef.DefList);
		Assert.NotEmpty(loadedDef.DefList);
		Assert.Collection(loadedDef.DefList.OrderBy(d => d.Key),
			d => {
				_ = Assert.IsType<MockDef2>(d);
				Assert.Equal("FirstDef", d.Key);
				Assert.Equal("SecondDef", ((MockDef2)d).SubDef.Key);
			},
			d => {
				_ = Assert.IsType<MockDef1>(d);
				Assert.Equal("SecondDef", d.Key);
				Assert.Equal(2, ((MockDef1)d).SampleValue1);
			}
		);
	}

	/// <summary>
	/// Tests that Load can initialize a Def with various field types, including primitives, enums, types, classes, and lists.
	/// </summary>
	[Fact]
	public void ParseDefTest14() {
		Init.Setup(5, service);

		MockDef9? loadedDef = service.LoadDef<MockDef9>("MockDef-comments");

		Assert.NotNull(loadedDef);
		Assert.Equal(1.2f, loadedDef.PrimitiveField);
		Assert.Equal(MockEnum.VALUE2, loadedDef.EnumField);
		Assert.Equal(typeof(int), loadedDef.TypeField);
		Assert.Equal(5, loadedDef.ClassField!.Value);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with an invalid enum value.
	/// </summary>
	[Fact]
	public void ParseDefTest15() {
		Init.Setup(5, service);

		Exception e = Assert.Throws<PropertyLoadException>(
			() => service.LoadDef<Def>("MockDef-invalid-enum")
		);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with an invalid type value.
	/// </summary>
	[Fact]
	public void ParseDefTest16() {
		Init.Setup(5, service);

		Exception e = Assert.Throws<UnresolvedTypeException>(
			() => service.LoadDef<Def>("MockDef-invalid-type")
		);
	}
	/// <summary>
	/// Tests that Load can successfully load a Def with a type field that meets the requirements of the EnforceInheritance attribute.
	/// </summary>
	[Fact]
	public void ParseDefTest17() {
		Init.Setup(5, service);

		MockDef10? loadedDef = service.LoadDef<MockDef10>("MockDef-inheritance-valid");

		Assert.NotNull(loadedDef);
		Assert.Equal(typeof(int), loadedDef.TypeField);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with a type that does not meet the requirements of the EnforceInheritance attribute.
	/// </summary>
	[Fact]
	public void ParseDefTest18() {
		Init.Setup(5, service);

		Exception e = Assert.Throws<DefInheritanceException>(
			() => service.LoadDef<Def>("MockDef-inheritance-invalid")
		);
	}
	/// <summary>
	/// Tests that Load throws an exception when trying to load a Def with a property that does not exist on the Def class.
	/// </summary>
	[Fact]
	public void ParseDefTest19() {
		Init.Setup(5, service);

		Exception e = Assert.Throws<MissingFieldException>(
			() => service.LoadDef<Def>("MockDef-invalid-prop")
		);
	}
	/// <summary>
	/// Tests that Load throws a <see cref="MissingDefPropException"/> when a required field is missing.
	/// </summary>
	[Fact]
	public void ParseDefTest20() {
		Init.Setup(11, service);
		service.Reload();

		Exception e = Assert.Throws<MissingDefPropException>(
			() => service.LoadDef<Def>("MockDef")
		);
	}

	/// <summary>
	/// Tests that defs can be loaded from multiple roots.
	/// </summary>
	[Fact]
	public void ParseDefTest21() {
		_ = service.RegisterResource(Init.MockDirectory(1), out _);
		_ = service.RegisterResource(Init.MockDirectory(2), out _);
		service.Reload();


		MockDef1? loadedDef1 = service.LoadDef<MockDef1>("MockDef");
		MockDef3? loadedDef2 = service.LoadDef<MockDef3>("ThirdDef");

		Assert.NotNull(loadedDef1);
		Assert.NotNull(loadedDef2);
	}

	/// <summary>
	/// Tests that defs can be loaded from multiple roots.
	/// </summary>
	[Fact]
	public void ParseDefTest22() {
		Init.Setup(13, service);

		MockDef9? def = service.LoadDef<MockDef9>("MockDef");

		Assert.NotNull(def);
		Assert.Equal(3, def.PrimitiveField);
		Assert.Equal(MockEnum.VALUE2, def.EnumField);
		Assert.NotNull(def.ClassField);
		Assert.Equal(1, def.ClassField.Value);
		Assert.NotNull(def.ListField);
		Assert.Collection(def.ListField,
			e => {
				Assert.Equal(4, e);
			},
			e => {
				Assert.Equal(5, e);
			},
			e => {
				Assert.Equal(6, e);
			}
		);
	}
	/// <summary>
	/// Tests that an exception is thrown when a class property on a Def has missing required properties.
	/// </summary>
	[Fact]
	public void ParseDefTest23() {
		Init.Setup(18, service);

		Exception ex1 = Assert.Throws<MissingDefPropException>(
			() => service.LoadDef<Def>("MockDef1")
		);
		Assert.NotNull(ex1);

		Exception ex2 = Assert.Throws<MissingDefPropException>(
			() => service.LoadDef<Def>("MockDef2")
		);
		Assert.NotNull(ex2);

		try {
			_ = service.LoadDef<Def>("MockDef4");
		}
		catch (MissingDefPropException) {
			Assert.True(false);
		}
	}
	/// <summary>
	/// Tests that ParseDef throws an exception when a Def fails validation.
	/// </summary>
	[Fact]
	public void ParseDefTest24() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"Lithium.Defs.Tests.MockDef17\"><Key>SampleDefKey</Key><Label>Label</Label></Def></Defs>");

		_ = service.RegisterResource(doc, out _);
		service.Reload();

		Exception ex = Assert.Throws<DefValidationException>(
			() => _ = service.LoadDef<MockDef17>("SampleDefKey")
		);
		Assert.NotNull(ex);
	}
	#endregion

	#region LoadFactory tests
	/// <summary>
	/// Tests that a Def can be loaded using a factory method marked with the DefFactory attribute.
	/// </summary>
	[Fact]
	public void LoadFactoryTest01() {
		Init.Setup(4, service);

		MockDef4? loadedDef = service.LoadDef<MockDef4>("MockDef01");

		Assert.NotNull(loadedDef);
		Assert.Equal(15, loadedDef.FactoryClass.tenPlus);
	}
	/// <summary>
	/// Tests that a Def can be loaded using a constructor marked with the DefConstructor attribute.
	/// </summary>
	[Fact]
	public void LoadFactoryTest02() {
		Init.Setup(4, service);

		MockDef5? loadedDef = service.LoadDef<MockDef5>("MockDef02");

		Assert.NotNull(loadedDef);
		Assert.Equal("test", loadedDef.FactoryClass.value);
	}
	/// <summary>
	/// Tests that an exception is thrown when a factory method or constructor does not take a single parameter of type XmlNode.
	/// </summary>
	[Fact]
	public void LoadFactoryTest04() {
		Init.Setup(4, service);

		Exception e = Assert.Throws<DefFactoryConstructorParamsException>(
			() => service.LoadDef<MockDef7>("MockDef04")
		);
	}
	/// <summary>
	/// Tests that an exception is thrown when a factory method or constructor returns a type that does not match the field it is being assigned to.
	/// </summary>
	[Fact]
	public void LoadFactoryTest05() {
		Init.Setup(4, service);

		Exception e = Assert.Throws<DefFactoryReturnTypeException>(
			() => service.LoadDef<MockDef8>("MockDef05")
		);
	}
	/// <summary>
	/// Tests that Factories can be loaded from separate static classes.
	/// </summary>
	[Fact]
	public void LoadFactoryTest06() {
		Init.Setup(17, service);

		ExtFactoryDef def = service.LoadDef<ExtFactoryDef>("MockDef1")!;

		Assert.NotNull(def);
		Assert.Equal("Content: information from XML", def.Data.Content);
	}
	/// <summary>
	/// Tests that the factory can be selected when there are multiple defined.
	/// </summary>
	[Fact]
	public void LoadFactoryTest07() {
		Init.Setup(17, service);

		ExtFactoryDef def = service.LoadDef<ExtFactoryDef>("MockDef2")!;

		Assert.NotNull(def);
		Assert.Equal("Different content: information from XML", def.Data.Content);
	}
	/// <summary>
	/// Tests that an exception is thrown when a factory class that doesn't exist is specified.
	/// </summary>
	[Fact]
	public void LoadFactoryTest08() {
		Init.Setup(17, service);

		Exception ex = Assert.Throws<UnresolvedTypeException>(
			() => service.LoadDef<ExtFactoryDef>("MockDef3")
		);

		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that an exception is thrown when a class defines multiple factories for a single type.
	/// </summary>
	[Fact]
	public void LoadFactoryTest09() {
		AssemblyLoadContext? context = new AssemblyLoadContext("TestAssemblyContext", true);

		try {
			_ = context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__test_assemblies__", "TestAssembly02", "Lithium.Defs.Tests.TestAssembly02.dll"));
			TypeChecker.assemblyScraper = new AssemblyScraper(AppDomain.CurrentDomain.GetAssemblies());
			Init.Setup(17, service);

			Exception ex = Assert.Throws<AmbiguousMatchException>(
				() => service.LoadDef<ExtFactoryDef>("MockDef1")
			);

			Assert.NotNull(ex);
		}
		finally {
			TypeChecker.assemblyScraper = new AssemblyScraper(AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name != "Lithium.Defs.Test.TestAssembly02"));
		}
	}
	#endregion
}
