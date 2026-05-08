using System;
using System.Text;
using System.Xml;

namespace Lithium.Core.Exceptions;

public class NodeMissingChildException(XmlNode node, string childName) : Exception {
	private readonly XmlNode node = node;
	private readonly string childName = childName;

	public override string Message {
		get {
			StringBuilder builder = new StringBuilder();
			builder.Append($"XML node missing '{childName}' child.");
			builder.AppendLine(node.InnerText);
			return builder.ToString();
		}
	}
}
