using System;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;
using Lithium.Defs.XML;
using Xunit;

namespace Lithium.Defs.Tests;

/// <summary>
/// Tests for Lithium.Defs.XML.DefXMLUtils.cs
/// </summary>
public class DefXMLUtilsTests {
	#region ValidateDefName
	/// <summary>
	/// Tests various valid Def key formats.
	/// </summary>
	[Theory]
	[InlineData("DefKey")]
	[InlineData("@1Def-Key")]
	[InlineData("feunsrivklkzjhkeg-sjrkdht__ajfesiungr")]
	[InlineData("A")]
	public void ValidateDefNameTest01(string key) {
		Assert.True(DefXMLUtils.ValidateDefName(key));
	}
	/// <summary>
	/// Tests various invalid Def key formats.
	/// </summary>
	[Theory]
	[InlineData("1DefKey")]
	[InlineData("-DefKey")]
	[InlineData("@1Def- Key")]
	[InlineData("#%$^&*(")]
	[InlineData("DefKey^ajflnegk")]
	public void ValidateDefNameTest02(string key) {
		Assert.False(DefXMLUtils.ValidateDefName(key));
	}
	#endregion

	#region InheritDefXML
	/// <summary>
	/// Tests that an already defined property can be overwritten.
	/// </summary>
	[Fact]
	public void InheritDefXMLTest01() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key><Property>1</Property><OtherProperty>2</OtherProperty></Def><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key><Property>replaced content</Property></Def></Defs>");
		XmlNode root = doc.FirstChild!;

		XmlNode child = root.ChildNodes[0]!;
		XmlNode parent = root.ChildNodes[1]!;

		DefXMLUtils.InheritDefXML(ref child, parent);

		Assert.Equal("replaced content", child.GetChildValue<string>("Property"));
		Assert.Equal("2", child.GetChildValue<string>("OtherProperty"));
	}
	/// <summary>
	/// Tests that a previously undefined property can be added.
	/// </summary>
	[Fact]
	public void InheritDefXMLTest02() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key><Property>1</Property></Def><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key><OtherProperty>OtherProperty from overwrite</OtherProperty></Def></Defs>");
		XmlNode root = doc.FirstChild!;

		XmlNode child = root.ChildNodes[0]!;
		XmlNode parent = root.ChildNodes[1]!;

		DefXMLUtils.InheritDefXML(ref child, parent);

		Assert.Equal("1", child.GetChildValue<string>("Property"));
		Assert.Equal("OtherProperty from overwrite", child.GetChildValue<string>("OtherProperty"));
	}
	/// <summary>
	/// Tests that the Key does not get overwritten.
	/// </summary>
	[Fact]
	public void InheritDefXMLTest03() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key></Def><Def Class=\"SampleClassName\"><Key>DifferentDefKey</Key></Def></Defs>");
		XmlNode root = doc.FirstChild!;

		XmlNode child = root.ChildNodes[0]!;
		XmlNode parent = root.ChildNodes[1]!;

		DefXMLUtils.InheritDefXML(ref child, parent);

		Assert.Equal("SampleDefKey", child.GetChildValue<string>("Key"));
	}
	/// <summary>
	/// Tests that the Key does not get overwritten.
	/// </summary>
	[Fact]
	public void InheritDefXMLTest04() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key></Def><Def Class=\"DifferentClassName\"><Key>SampleDefKey</Key></Def></Defs>");
		XmlNode root = doc.FirstChild!;

		XmlNode child = root.ChildNodes[0]!;
		XmlNode parent = root.ChildNodes[1]!;

		Exception ex = Assert.Throws<DefParentInvalidException>(
			() => DefXMLUtils.InheritDefXML(ref child, parent)
		);
		Assert.NotNull(ex);
	}
	#endregion

	#region CreateTempDef
	/// <summary>
	/// Tests that a temporary Def instance with an indicative name can be created from XML.
	/// </summary>
	[Fact]
	public void CreateTempDefTest01() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"Lithium.Defs.Tests.MockDef1\"><Key>SampleDefKey</Key></Def></Defs>");
		XmlNode node = doc.FirstChild!.FirstChild!;

		Def def = DefXMLUtils.CreateTempDef(node);

		Assert.Equal(typeof(MockDef1), def.GetType());
		Assert.Contains('^', def.Key);
		Assert.Null(def.Label);
		Assert.Equal(default, ((MockDef1)def).SampleValue1);
	}
	/// <summary>
	/// Tests than an exception is thrown if the XML calls for a class that does not exist.
	/// </summary>
	[Fact]
	public void CreateTempDefTest02() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"Fake.Class.Name\"><Key>SampleDefKey</Key></Def></Defs>");
		XmlNode node = doc.FirstChild!.FirstChild!;

		Exception ex = Assert.Throws<UnresolvedTypeException>(
			() => DefXMLUtils.CreateTempDef(node)
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests than an exception is thrown if the XML calls for a class that does not inherit from Lithium.Defs.Def.
	/// </summary>
	[Fact]
	public void CreateTempDefTest03() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"System.Int32\"><Key>SampleDefKey</Key></Def></Defs>");
		XmlNode node = doc.FirstChild!.FirstChild!;

		Exception ex = Assert.Throws<DefInheritanceException>(
			() => DefXMLUtils.CreateTempDef(node)
		);
		Assert.NotNull(ex);
	}
	#endregion

	#region GetDefKey
	/// <summary>
	/// Tests that GetDefKey loads the text from a node's Key child.
	/// </summary>
	[Fact]
	public void GetDefKeyTest01() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"SampleClassName\"><Key>SampleDefKey</Key></Def></Defs>");
		XmlNode node = doc.FirstChild!.FirstChild!;

		string key = DefXMLUtils.GetDefKey(node);

		Assert.Equal("SampleDefKey", key);
	}
	/// <summary>
	/// Tests that GetDefKey throws an exception if the node is missing the Key child.
	/// </summary>
	[Fact]
	public void GetDefKeyTest02() {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml("<Defs><Def Class=\"SampleClassName\"></Def></Defs>");
		XmlNode node = doc.FirstChild!.FirstChild!;

		Exception ex = Assert.Throws<NodeMissingChildException>(
			() => DefXMLUtils.GetDefKey(node)
		);
		Assert.NotNull(ex);
	}
	#endregion
}
