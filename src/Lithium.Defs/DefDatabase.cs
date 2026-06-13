using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;

namespace Lithium.Defs;

/// <summary>
/// Utilities for accessing Def values.
/// </summary>
public static class DefDatabase {
	private static Dictionary<string, XmlNode> XmlDefinitions { get; } = new Dictionary<string, XmlNode>();
	private static Dictionary<string, Def> ParsedDefinitions { get; } = new Dictionary<string, Def>();

	/// <summary>
	/// Initializes the DefDatabase by loading all XML files from the defined root directory.
	/// </summary>
	/// <param name="defFiles">
	/// Absolute paths to the XML files to load.
	/// If null, automatically gathers them from the added root directories in <see cref="Settings"/>
	/// </param>
	/// <exception cref="FileNotFoundException">Thrown if any supplied file path does not exist.</exception>
	/// <exception cref="FileLoadException">Thrown if the specified any is not a .xml file.</exception>
	/// <exception cref="XmlException">Thrown if the any contents cannot be parsed as valid XML.</exception>
	public static void Initialize(params string[] defFiles) {
		if (defFiles.Length == 0) {
			if (Settings.DefRootDirectories.Count == 0 && Settings.EmbeddedResources.Count == 0) {
				throw new ResourceRootDirectoryMissingException("Def");
			}
			defFiles = Settings.DefRootDirectories.Select(XmlLoader.GetAllFiles).SelectMany(f => f).ToArray();
		}

		DefParser.defLinks.Clear();
		XmlDefinitions.Clear();
		ParsedDefinitions.Clear();

		foreach (Assembly assembly in Settings.EmbeddedResources.Keys) {
			IEnumerable<string> resources = Settings.EmbeddedResources[assembly];
			foreach (string resource in resources) {
				XmlDocument? doc = XmlLoader.LoadDocument(ResourceLoader.LoadResourceStream(assembly, resource));
				if (doc == null) {
					continue;
				}
				ParseXmlDoc(doc);
			}
		}

		foreach (string path in defFiles) {
			XmlDocument doc = XmlLoader.LoadDocument(path);
			ParseXmlDoc(doc);
		}

		if (!Settings.DeferredParsing) {
			// Immediately load all XML nodes.
			IEnumerable<XmlNode> defNodes = GetAllNodes();
			foreach (XmlNode node in defNodes) {
				_ = DefParser.ParseDef(node);
			}

			DefParser.ResolveDefLinks();
			XmlDefinitions.Clear();
		}
	}

	private static void ParseXmlDoc(XmlDocument doc) {
		XmlNode? defsNode = doc.SelectSingleNode(Constants.DEFS_ROOT_NODE);
		// Skip files that don't contain defs.
		if (defsNode == null) {
			return;
		}
		foreach (XmlNode child in defsNode.ChildNodes) {
			// Skip comment nodes.
			if (child.NodeType == XmlNodeType.Comment) {
				continue;
			}

			AddToDB(child);
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
		_ = XmlDefinitions.TryAdd(GetDefKey(node), node);
	}
	/// <summary>
	/// Adds a new def to the DefDatabase, using the def's 'key' property as the key in the database.
	/// </summary>
	/// <param name="def">The def to add to the database. Must have a unique 'key' property.</param>
	internal static void AddToDB(Def def) {
		_ = ParsedDefinitions.TryAdd(def.Key, def);
	}

	/// <summary>
	/// Returns the value of the 'key' child element of the provided XML node. Throws an exception if the node does not contain a 'key' child element.
	/// </summary>
	/// <param name="node">The XML node to extract the key from. Must contain a 'key' child element.</param>
	/// <returns>The value of the 'key' child element of the provided XML node.</returns>
	/// <exception cref="NodeMissingChildException">Thrown if the provided XML node does not contain a 'Key' child element.</exception>
	internal static string GetDefKey(XmlNode node) {
		XmlNode? keyNode = node.SelectSingleNode(Constants.DEF_KEY_ELEMENT);
		if (keyNode == null) {
			throw new NodeMissingChildException(node, Constants.DEF_KEY_ELEMENT);
		}
		return keyNode.InnerText;
	}

	/// <summary>
	/// Returns the XML node associated with the provided key in the DefDatabase. Throws an exception if no node with the provided key exists in the database.
	/// </summary>
	/// <param name="key">The key of the XML node to return.</param>
	/// <returns>The XML node associated with the provided key in the DefDatabase.</returns>
	/// <exception cref="DefNotFoundException">Thrown if no node with the provided key exists in the XML cache.</exception>
	internal static XmlNode LoadXml(string key) {
		if (!XmlDefinitions.TryGetValue(key, out XmlNode? value)) {
			throw new DefNotFoundException(key);
		}
		return value;
	}
	/// <summary>
	/// Returns the def associated with the provided key in the DefDatabase.
	/// Returns null if no def with the provided key exists in the database or if the def associated with the provided key is not of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of the def to return.</typeparam>
	/// <param name="key">The key of the def to return.</param>
	/// <returns>The def associated with the provided key in the DefDatabase, or null if not available.</returns>
	public static T? Load<T>(string key) where T : Def {
		Def? loadedDef = Settings.DeferredParsing ? LoadDeferred<T>(key) : LoadDirect(key) as Def;
		if (loadedDef is null or not T) {
			return null;
		}

		return loadedDef as T;
	}
	/// <summary>
	/// Dynamically load a def from XML which has not already been loaded.
	/// </summary>
	/// <typeparam name="T">The type of the def to return.</typeparam>
	/// <param name="key">The key of the def to return.</param>
	/// <returns>The def as loaded from XML, or null if it either has been loaded already or could not be found.</returns>
	internal static T? LoadDeferred<T>(string key) where T : Def {
		// Dynamically parse if it hasn't been loaded yet.
		if (XmlDefinitions.TryGetValue(key, out XmlNode? loadedNode)) {
			// Parse the def.
			Def def = DefParser.ParseDef(loadedNode);
			DefParser.ResolveDefLinks();
			if (def is not T) {
				return null;
			}
			// Store the parsed value.
			AddToDB(def);
			// Clean up the XML, which should no longer be needed.
			_ = XmlDefinitions.Remove(key);
			return def as T;
		}
		return null;
	}
	/// <summary>
	/// Directly load a pre-loaded def.
	/// </summary>
	/// <param name="key">The key of the def to return.</param>
	/// <returns>The def as loaded from the database, or null if it is not loaded.</returns>
	internal static object? LoadDirect(string key) {
		if (ParsedDefinitions.TryGetValue(key, out Def? value)) {
			return value;
		}
		return null;
	}
}
