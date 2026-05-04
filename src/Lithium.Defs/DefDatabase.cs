using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Lithium.Core;

namespace Lithium.Defs;

public static class DefDatabase {
	private static Dictionary<string, XmlNode> XmlDefinitions { get; } = new Dictionary<string, XmlNode>();
	private static Dictionary<string, Def> ParsedDefinitions { get; } = new Dictionary<string, Def>();

	/// <summary>
	/// Initializes the DefDatabase by loading all XML files from the defined root directory.
	/// </summary>
	/// <param name="defFiles">Absolute paths to the XML files to load.</param>
	internal static void Initialize(IEnumerable<string> defFiles) {
		XmlDefinitions.Clear();
		ParsedDefinitions.Clear();

		foreach (string path in defFiles) {
			XmlDocument doc = XmlLoader.LoadDocument(path);

			XmlNode? defsNode = doc.SelectSingleNode("/Defs");
			// Skip files that don't contain defs.
			if (defsNode == null) {
				continue;
			}
			foreach (XmlNode child in defsNode.ChildNodes) {
				/// Skip comment nodes.
				if (child.NodeType == XmlNodeType.Comment) {
					continue;
				}

				AddToDB(child);
			}
		}
	}

	/// <summary>
	/// Returns all XML nodes currently stored in the DefDatabase.
	/// </summary>
	/// <returns>An enumerable of all XML nodes currently stored in the DefDatabase.</returns>
	internal static IEnumerable<XmlNode> GetAllNodes() {
		return XmlDefinitions.Values;
	}
	/// <summary>
	/// Returns all defs currently stored in the DefDatabase.
	/// </summary>
	/// <returns>An enumerable of all defs currently stored in the DefDatabase.</returns>
	public static IEnumerable<Def> LoadAll() {
		return ParsedDefinitions.Values;
	}

	/// <summary>
	/// Adds a new XML node to the DefDatabase, using the node's 'key' child element as the key in the database.
	/// </summary>
	/// <param name="node">The XML node to add to the database. Must contain a 'key' child element.</param>
	private static void AddToDB(XmlNode node) {
		XmlDefinitions.TryAdd(GetDefKey(node), node);
	}
	/// <summary>
	/// Adds a new def to the DefDatabase, using the def's 'key' property as the key in the database.
	/// </summary>
	/// <param name="def">The def to add to the database. Must have a unique 'key' property.</param>
	internal static void AddToDB(Def def) {
		ParsedDefinitions.TryAdd(def.Key, def);
	}

	/// <summary>
	/// Returns the value of the 'key' child element of the provided XML node. Throws an exception if the node does not contain a 'key' child element.
	/// </summary>
	/// <param name="node">The XML node to extract the key from. Must contain a 'key' child element.</param>
	/// <returns>The value of the 'key' child element of the provided XML node.</
	internal static string GetDefKey(XmlNode node) {
		XmlNode? keyNode = node.SelectSingleNode("key");
		if (keyNode == null) {
			throw new Exception("Def node missing 'key' child element.");
		}
		return keyNode.InnerText;
	}

	/// <summary>
	/// Returns the XML node associated with the provided key in the DefDatabase. Throws an exception if no node with the provided key exists in the database.
	/// </summary>
	/// <param name="key">The key of the XML node to return.</param>
	/// <returns>The XML node associated with the provided key in the DefDatabase.</returns>
	internal static XmlNode LoadXml(string key) {
		if (!XmlDefinitions.TryGetValue(key, out XmlNode? value)) {
			throw new Exception($"No Def was found with the key '{key}'.");
		}
		return value;
	}
	/// <summary>
	/// Returns the def associated with the provided key in the DefDatabase. Returns null if no def with the provided key exists in the database or if the def associated with the provided key is not of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of the def to return.</typeparam>
	/// <param name="key">The key of the def to return.</param>
	/// <returns>The def associated with the provided key in the DefDatabase, or null if not available.</returns>
	public static T? Load<T>(string key) where T : Def {
		if (!ParsedDefinitions.TryGetValue(key, out Def? value)) {
			return null;
		}
		if (value is not T) {
			return null;
		}
		return value as T;
	}
}
