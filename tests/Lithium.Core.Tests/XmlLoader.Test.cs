using System.Xml;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Lithium.Core.Tests;

/// <summary>
/// Tests for Lithium.Core.XmlLoader.cs
/// </summary>
public class XmlLoaderTests {
	private static readonly string mockXmlDirectory = Path.Combine(AppContext.BaseDirectory, "__mocks__");
	private static readonly string mockXmlFile1 = Path.Combine(AppContext.BaseDirectory, "__mocks__", "XmlMock01.xml");

	private static readonly string[] mockXmlFiles1 = new string[] {
		mockXmlFile1,
		Path.Combine(mockXmlDirectory, "sub", "a.xml"),
		Path.Combine(mockXmlDirectory, "sub", "dir", "c.xml")
	};
	private static readonly string[] mockXmlFiles2 = new string[] {
		Path.Combine(mockXmlDirectory, "sub", "a.xml"),
		Path.Combine(mockXmlDirectory, "sub", "dir", "c.xml")
	};

	#region GetAllFiles tests
	/// <summary>
	/// Tests that the all XML files are returned for a given directory and subdirectories.
	/// </summary>
	[Fact]
	public void GetAllFilesTest01() {
		IEnumerable<string> files = XmlLoader.GetAllFiles(mockXmlDirectory);

		Assert.Equal(mockXmlFiles1.Length, files.Count());
		for (int i = 0; i < files.Count(); i++) {
			Assert.Equal(mockXmlFiles1[i], files.ElementAt(i));
		}
	}
	/// <summary>
	/// Tests that the correct XML files are returned for a given subdirectory.
	/// </summary>
	[Fact]
	public void GetAllFilesTest02() {
		IEnumerable<string> files = XmlLoader.GetAllFiles(Path.Combine(AppContext.BaseDirectory, "__mocks__", "sub"));

		Assert.Equal(mockXmlFiles2.Length, files.Count());
		for (int i = 0; i < files.Count(); i++) {
			Assert.Equal(mockXmlFiles2[i], files.ElementAt(i));
		}
	}
	/// <summary>
	/// Tests that a FileNotFoundException is thrown when the specified directory does not exist.
	/// </summary>
	[Fact]
	public void GetAllFilesTest03() {
		Exception? ex = Assert.Throws<DirectoryNotFoundException>(
			() => XmlLoader.GetAllFiles(
				Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
			)
		);
	}
	#endregion

	#region LoadDocument tests
	/// <summary>
	/// Tests that an XML document is correctly loaded from a valid file path and that the contents match the expected XML structure.
	/// </summary>
	[Fact]
	public void LoadDocumentTest01() {
		XmlDocument doc = XmlLoader.LoadDocument(mockXmlFile1);
		string xml = doc.OuterXml;

		Assert.Equal(
			"""<Defs><MockDef><key>MockDef</key><label>MockDef_Label</label><sampleValue1>1</sampleValue1></MockDef></Defs>""",
			xml
		);
	}
	/// <summary>
	/// Tests that a FileNotFoundException is thrown when attempting to load an XML document from a non-existent file path.
	/// </summary>
	[Fact]
	public void LoadDocumentTest02() {
		Exception? ex = Assert.Throws<FileNotFoundException>(
			() => XmlLoader.LoadDocument(
				Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), ".xml")
			)
		);
	}
	/// <summary>
	/// Tests that a FileLoadException is thrown when attempting to load an XML document from a file that is not a valid XML file.
	/// </summary>
	[Fact]
	public void LoadDocumentTest03() {
		Exception? ex = Assert.Throws<FileLoadException>(
			() => XmlLoader.LoadDocument(
				Path.Combine(mockXmlDirectory, "sub", "b.dat")
			)
		);
	}
	#endregion

	#region GetChildValue tests
	/// <summary>
	/// Tests that the GetChildValue method correctly retrieves and converts the value of a child XML node.
	/// </summary>
	[Fact]
	public void GetChildValueTest01() {
		XmlDocument doc = XmlLoader.LoadDocument(mockXmlFile1);
		XmlNode root = doc.DocumentElement!;
		XmlNode node = root.FirstChild!;

		string? defKey = node.GetChildValue<string>("key");
		int? sampleValue = node.GetChildValue<int>("sampleValue1");

		Assert.Equal("MockDef", defKey);
		Assert.Equal(1, sampleValue);
	}
	/// <summary>
	/// Tests that the GetChildValue method returns null when the specified child node does not exist.
	/// </summary>
	[Fact]
	public void GetChildValueTest02() {
		XmlDocument doc = XmlLoader.LoadDocument(mockXmlFile1);
		XmlNode root = doc.DocumentElement!;
		XmlNode node = root.FirstChild!;

		object? value = node.GetChildValue<object>("nodeThatDoesNotExist");

		Assert.Null(value);
	}
	/// <summary>
	/// Tests that the GetChildValue method correctly outputs the child XML node when it exists..
	/// </summary>
	[Fact]
	public void GetChildValueTest03() {
		XmlDocument doc = XmlLoader.LoadDocument(mockXmlFile1);
		XmlNode root = doc.DocumentElement!;
		XmlNode node = root.FirstChild!;

		_ = node.GetChildValue<int>("sampleValue1", out XmlNode? child);

		Assert.NotNull(child);
		Assert.Equal("1", child.InnerText);
	}
	/// <summary>
	/// Tests that the GetChildValue method returns null and outputs null when the specified child node does not exist.
	/// </summary>
	[Fact]
	public void GetChildValueTest04() {
		XmlDocument doc = XmlLoader.LoadDocument(mockXmlFile1);
		XmlNode root = doc.DocumentElement!;
		XmlNode node = root.FirstChild!;

		object? value = node.GetChildValue<object>("nodeThatDoesNotExist", out XmlNode? child);

		Assert.Null(child);
		Assert.Null(value);
	}
	#endregion
}
