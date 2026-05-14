using System;
using System.Xml;

namespace Lithium.Core.Exceptions;

/// <summary>
/// Exception indicating an XML node is missing a required child element.
/// </summary>
/// <param name="childName">The node with the missing child.</param>
/// <param name="node">The name of the expected child node.</param>
public class NodeMissingChildException(XmlNode node, string childName) : Exception {
	private readonly XmlNode node = node;
	private readonly string childName = childName;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"XML node missing '{childName}' child.\n{node.OuterXml}";
}
