using System;
using System.IO;
using System.Xml;
using Lithium.Core.Exceptions;
using Xunit;

namespace Lithium.Core.Tests;

/// <summary>
/// Tests for Lithium.Core.Exceptions.NodeMissingChildException.cs
/// </summary>
public class NodeMissingChildExceptionTests {
	private static readonly string mockXmlFile1 = Path.Combine(AppContext.BaseDirectory, "__mocks__", "XmlMock01.xml");

	/// <summary>
	/// Tests that the constructor creates the expected message.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		XmlNode node = XmlLoader.LoadDocument(mockXmlFile1)!.FirstChild!.FirstChild!;

		NodeMissingChildException ex = new NodeMissingChildException(node, "Property");

		Assert.Equal(
			"XML node missing 'Property' child.\n<MockDef><key>MockDef</key><label>MockDef_Label</label><sampleValue1>1</sampleValue1></MockDef>",
			ex.Message
		);
	}
}
