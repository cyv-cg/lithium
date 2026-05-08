using System;
using System.Text;
using System.Xml;

namespace Lithium.Core.Exceptions;

public class NodeMissingChildException(XmlNode node, string childName) : Exception {
	private readonly XmlNode node = node;
	private readonly string childName = childName;

	public override string Message => $"XML node missing '{childName}' child.\n{node.OuterXml}";
}
