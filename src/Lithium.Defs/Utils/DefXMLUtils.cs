using System;
using System.Reflection;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;
using Lithium.Defs.Utils;

namespace Lithium.Defs.XML;

/// <summary>
/// Utility methods for Def XML nodes.
/// </summary>
public static partial class DefXMLUtils {
	/// <summary>
	/// Validates a Def key against a regular expression.
	/// </summary>
	/// <param name="key">Def key to check.</param>
	/// <returns>True if the key passes, false otherwise.</returns>
	public static bool ValidateDefName(string key) {
		return DefUtils.DefKeyRegex().Count(key) > 0;
	}

	/// <summary>
	/// Utility for applying Def inheritance at the XML-level.
	/// Used for when a single Def key is defined in multiple places.
	/// The Def loaded later overwrites the one loaded sooner.
	/// </summary>
	/// <param name="child">Reference to the child node. This is the node that will have its data overwritten.</param>
	/// <param name="parent">The parent node and source of the data to overwrite with.</param>
	/// <exception cref="DefParentInvalidException">Thrown if the parent and child are not the same type.</exception>
	public static void InheritDefXML(ref XmlNode child, XmlNode parent) {
		// Validate that the parent and child reference the same type name.
		if (!child.GetAttributeValue(Constants.DEF_CLASS_ATTR).Equals(parent.GetAttributeValue(Constants.DEF_CLASS_ATTR))) {
			throw new DefParentInvalidException(GetDefKey(child));
		}
		// Copy all properties from the parent onto the child, overwriting existing data.
		foreach (XmlNode childOverride in parent.ChildNodes) {
			string nodeProp = childOverride.Name;
			// Skip the key property, since those should match anyway.
			if (nodeProp.Equals(Constants.DEF_KEY_ELEMENT)) {
				continue;
			}
			// If the child node already has this property defined, just replace the inner XML.
			XmlNode? toReplace = child.SelectSingleNode(nodeProp);
			if (toReplace != null) {
				toReplace.InnerXml = childOverride.InnerXml;
			}
			// If it doesn't have that property already, import the parent's node.
			else {
				// child.OwnerDocument only returns null if child is itself of type XmlDocument.
				// That *should* never happen with this setup and syntax.
				XmlNode importedNodeToAdd = child.OwnerDocument!.ImportNode(childOverride, true);
				_ = child.AppendChild(importedNodeToAdd);
			}
		}
	}

	/// <summary>
	/// Creates a temporary Def instance from an XML node.
	/// </summary>
	/// <param name="node">The XML node to create the Def from.</param>
	/// <returns>A temporary Def instance.</returns>
	/// <remarks>
	/// The temporary Def's key will be in the format of <c>{key}^{uuid}</c>, where {key} is the value of the "Key" element and {uuid} is a new UUID.
	/// </remarks>
	/// <exception cref="UnresolvedTypeException">Thrown if the Def's class cannot be resolved to a type.</exception>
	/// <exception cref="DefInheritanceException">Thrown if the resolved type does not inherit from <see cref="Def"/>.</exception>
	public static Def CreateTempDef(XmlNode node) {
		string @class = node.GetAttributeValue(Constants.DEF_CLASS_ATTR);
		Type? defType = TypeChecker.ResolveType(@class);
		if (defType == null) {
			throw new UnresolvedTypeException(@class);
		}
		string key = GetDefKey(node);
		if (!defType.IsDef()) {
			throw new DefInheritanceException(key, defType, typeof(Def));
		}
		object instance = Activator.CreateInstance(defType)!;
		PropertyInfo prop = typeof(Def).GetProperty(Constants.DEF_KEY_ELEMENT)!;
		prop.SetValue(instance, $"{key}{Constants.TEMP_DEF_INDICATOR}{Guid.NewGuid()}");
		return (Def)instance;
	}

	/// <summary>
	/// Attemps to get the name of a Def from its XML.
	/// </summary>
	/// <param name="node">Top-level Def node to check.</param>
	/// <returns>The value of the Def's "Key" child.</returns>
	/// <exception cref="NodeMissingChildException">Thrown if the "Key" element does not exist.</exception>
	public static string GetDefKey(XmlNode node) {
		string? key = node.GetChildValue<string>(Constants.DEF_KEY_ELEMENT);
		if (string.IsNullOrEmpty(key)) {
			throw new NodeMissingChildException(node, Constants.DEF_KEY_ELEMENT);
		}
		return key;
	}
}
