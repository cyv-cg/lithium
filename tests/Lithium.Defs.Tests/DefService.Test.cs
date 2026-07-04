using System;
using System.IO;
using System.Linq;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.DefService
/// </summary>
public class DefServiceTests {
	private DefService service;

	/// <summary>
	/// Reset the service between runs.
	/// </summary>
	public DefServiceTests() {
		service = new DefService(
			new DefServiceOptions {
				DeferredLoad = true
			}
		);
	}

	#region RegisterResource
	/// <summary>
	/// Tests that a directory gets registered successfully.
	/// </summary>
	[Fact]
	public void RegisterResourceTest01() {
		bool success = service.RegisterResource(Init.MockDirectory(1));
		service.Reload();

		_ = service.LoadAll();

		Assert.True(success);
		Assert.Collection(service.defs,
			d => {
				Assert.Equal("MockDef", d.Key);
			}
		);
	}
	/// <summary>
	/// Tests that an assembly gets registered successfully.
	/// </summary>
	[Fact]
	public void RegisterResourceTest02() {
		bool success = service.RegisterResource(typeof(DefServiceTests).Assembly);
		service.Reload();

		_ = service.LoadAll();

		Assert.True(success);
		Assert.Collection(service.defs,
			d => {
				Assert.Equal("MockDef", d.Key);
			}
		);
	}
	/// <summary>
	/// Tests that an XmlDocument gets registered successfully.
	/// </summary>
	[Fact]
	public void RegisterResourceTest03() {
		XmlDocument doc = XmlLoader.LoadDocument(Path.Combine(Init.MockDirectory(1), "mockDefs.xml"));
		bool success = service.RegisterResource(doc);
		service.Reload();

		_ = service.LoadAll();

		Assert.True(success);
		Assert.Collection(service.defs,
			d => {
				Assert.Equal("MockDef", d.Key);
			}
		);
	}
	/// <summary>
	/// Tests that an ArgumentNullException is thrown when the directory name is empty.
	/// </summary>
	[Fact]
	public void RegisterResourceTest04() {
		bool success = false;
		Exception ex = Assert.Throws<ArgumentNullException>(
			() => success = service.RegisterResource("")
		);
		Assert.False(success);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that a DirectoryNotFoundException is thrown when the directory doesn't exist.
	/// </summary>
	[Fact]
	public void RegisterResourceTest05() {
		bool success = false;
		Exception ex = Assert.Throws<DirectoryNotFoundException>(
			() => success = service.RegisterResource(Guid.NewGuid().ToString())
		);
		Assert.False(success);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that the registration fails if the assembly has no applicable files.
	/// </summary>
	[Fact]
	public void RegisterResourceTest06() {
		bool success = service.RegisterResource(typeof(DefService).Assembly);
		service.Reload();

		Assert.False(success);
		Assert.Empty(service.resources);
	}
	/// <summary>
	/// Tests that the registration fails when adding the same resource again.
	/// </summary>
	[Fact]
	public void RegisterResourceTest07() {
		bool success1 = service.RegisterResource(Init.MockDirectory(1));
		bool success2 = service.RegisterResource(typeof(DefServiceTests).Assembly);
		bool success3 = service.RegisterResource(XmlLoader.LoadDocument(Path.Combine(Init.MockDirectory(1), "mockDefs.xml")));

		Assert.True(success1);
		Assert.True(success2);
		Assert.True(success3);

		bool _success1 = service.RegisterResource(Init.MockDirectory(1));
		bool _success2 = service.RegisterResource(typeof(DefServiceTests).Assembly);
		bool _success3 = service.RegisterResource(XmlLoader.LoadDocument(Path.Combine(Init.MockDirectory(1), "mockDefs.xml")));

		Assert.False(_success1);
		Assert.False(_success2);
		Assert.False(_success3);
	}
	/// <summary>
	/// Tests that registration fails for an assembly if it contains an empty file.
	/// </summary>
	[Fact]
	public void RegisterResourceTest08() {
		bool success = service.RegisterResource(typeof(TestAssembly01.TestAssembly01Class).Assembly);

		Assert.False(success);
		Assert.Empty(service.resources);
	}
	#endregion

	#region ParseDocument
	/// <summary>
	/// Tests that disabled XML Defs do not get read.
	/// </summary>
	[Fact]
	public void ParseDocumentTest01() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"Lithium.Defs.Def\"><Key>SampleDefKey</Key><Disabled>true</Disabled></Def></Defs>");

		_ = service.RegisterResource(doc);
		service.Reload();

		Assert.Empty(service.resources);
	}
	/// <summary>
	/// Tests that documents missing the top-level Defs node do not get read.
	/// </summary>
	[Fact]
	public void ParseDocumentTest02() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Def Class=\"Lithium.Defs.Def\"><Key>SampleDefKey</Key></Def>");

		_ = service.RegisterResource(doc);
		service.Reload();

		Assert.Empty(service.resources);
	}
	/// <summary>
	/// Tests that an exception is thrown if an XML Def is missing the key element.
	/// </summary>
	[Fact]
	public void ParseDocumentTest03() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"Lithium.Defs.Def\"></Def></Defs>");

		_ = service.RegisterResource(doc);
		Exception ex = Assert.Throws<NodeMissingChildException>(
			service.Reload
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that an exception is thrown if an XML Def has an invalid key.
	/// </summary>
	[Fact]
	public void ParseDocumentTest04() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"Lithium.Defs.Def\"><Key>invalid key name</Key></Def></Defs>");

		_ = service.RegisterResource(doc);
		Exception ex = Assert.Throws<FormatException>(
			service.Reload
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that XML nodes inheritance occurs when overloading an XML Def.
	/// </summary>
	[Fact]
	public void ParseDocumentTest05() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key><Property>1</Property><OtherProperty>2</OtherProperty></Def><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key><Property>replaced content</Property></Def></Defs>");

		_ = service.RegisterResource(doc);
		service.Reload();

		XmlNode node = service.resources["SampleDefKey"];
		Assert.Equal("replaced content", node.GetChildValue<string>("Property"));
		Assert.Equal("2", node.GetChildValue<string>("OtherProperty"));
	}
	#endregion

	#region LoadAll
	/// <summary>
	/// Tests that LoadAll loads all Defs exactly matching the specified type and can dynamically parse unloaded Defs.
	/// </summary>
	[Fact]
	public void LoadAllTest01() {
		_ = service.RegisterResource(Init.MockDirectory(14));
		service.Reload();

		MockDef1[] defs1 = service.LoadAll<MockDef1>().OrderBy(d => d.Key).ToArray();
		MockDef2[] defs2 = service.LoadAll<MockDef2>().OrderBy(d => d.Key).ToArray();
		Def[] defs3 = service.LoadAll<Def>().OrderBy(d => d.Key).ToArray();

		Assert.Collection(defs1,
			d => {
				Assert.Equal("MockDef1", d.Key);
			},
			d => {
				Assert.Equal("MockDef2", d.Key);
			}
		);

		Assert.Collection(defs2,
			d => {
				Assert.Equal("MockDef3", d.Key);
			}
		);

		Assert.Empty(defs3);
	}
	/// <summary>
	/// Tests that LoadAll loads all Defs exactly matching the specified type.
	/// </summary>
	[Fact]
	public void LoadAllTest02() {
		service = new DefService(
			new DefServiceOptions {
				DeferredLoad = false
			}
		);
		_ = service.RegisterResource(Init.MockDirectory(14));
		service.Reload();

		MockDef1[] defs1 = service.LoadAll<MockDef1>().OrderBy(d => d.Key).ToArray();
		MockDef2[] defs2 = service.LoadAll<MockDef2>().OrderBy(d => d.Key).ToArray();
		Def[] defs3 = service.LoadAll<Def>().OrderBy(d => d.Key).ToArray();

		Assert.Collection(defs1,
			d => {
				Assert.Equal("MockDef1", d.Key);
			},
			d => {
				Assert.Equal("MockDef2", d.Key);
			}
		);

		Assert.Collection(defs2,
			d => {
				Assert.Equal("MockDef3", d.Key);
			}
		);

		Assert.Empty(defs3);
	}
	/// <summary>
	/// Tests that LoadAll with deferred loading skips checking XML Defs without a defined class.
	/// </summary>
	[Fact]
	public void LoadAllTest03() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def><Key>SampleDefKey</Key></Def></Defs>");

		_ = service.RegisterResource(doc);
		service.Reload();

		Def[] defs = service.LoadAll<Def>().ToArray();

		Assert.Empty(defs);
	}
	#endregion

	#region TryLoadDef
	/// <summary>
	/// Tests that TryLoadDef can correctly load a Def that exists.
	/// </summary>
	[Fact]
	public void TryLoadDefTest01() {
		_ = service.RegisterResource(Init.MockDirectory(2));
		service.Reload();

		bool success = service.TryLoadDef("FirstDef", out MockDef2? def);

		Assert.True(success);
		Assert.NotNull(def);
		Assert.Equal("FirstDef", def.Key);
	}
	/// <summary>
	/// Tests that TryLoadDef returns false if the requested Def is the wrong type.
	/// </summary>
	[Fact]
	public void TryLoadDefTest02() {
		_ = service.RegisterResource(Init.MockDirectory(2));
		service.Reload();

		bool success = service.TryLoadDef("FirstDef", out MockDef1? def);

		Assert.False(success);
		Assert.Null(def);
	}
	/// <summary>
	/// Tests that TryLoadDef returns false if the requested Def does not exist.
	/// </summary>
	[Fact]
	public void TryLoadDefTest03() {
		_ = service.RegisterResource(Init.MockDirectory(2));
		service.Reload();

		bool success = service.TryLoadDef("DefThatDoesNotExist", out MockDef2? def);

		Assert.False(success);
		Assert.Null(def);
	}
	#endregion

	#region LoadDef
	/// <summary>
	/// Tests that when loading a registered Def key, it is successful.
	/// </summary>
	[Fact]
	public void LoadDefTest01() {
		_ = service.RegisterResource(Init.MockDirectory(2));
		service.Reload();

		MockDef2? def = service.LoadDef<MockDef2>("FirstDef");

		Assert.NotNull(def);
		Assert.Equal("FirstDef", def.Key);
	}
	/// <summary>
	/// Tests that when loading a Def that exists but with the wrong type, LoadDef returns null.
	/// </summary>
	[Fact]
	public void LoadDefTest02() {
		_ = service.RegisterResource(Init.MockDirectory(2));
		service.Reload();

		MockDef1? def = service.LoadDef<MockDef1>("FirstDef");

		Assert.Null(def);
	}
	/// <summary>
	/// Tests that LoadDef throws an exception when requesting a Def that does not exist.
	/// </summary>
	[Fact]
	public void LoadDefTest03() {
		_ = service.RegisterResource(Init.MockDirectory(2));
		service.Reload();

		Exception ex = Assert.Throws<DefNotFoundException>(
			() => service.LoadDef<MockDef2>("DefThatDoesNotExist")
		);
		Assert.NotNull(ex);
	}
	#endregion
}
