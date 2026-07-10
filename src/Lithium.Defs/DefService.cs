
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;
using Lithium.Defs.Utils;
using Lithium.Defs.XML;

namespace Lithium.Defs;

/// <summary>
/// Service to handle the fetching of Def resources.
/// </summary>
public class DefService : IDefService, IResourceRegistry<string>, IResourceRegistry<Assembly>, IResourceRegistry<XmlDocument> {
	private readonly DefServiceOptions options;

	/// <summary>
	/// Unprocessed XML documents.
	/// </summary>
	private readonly Dictionary<string, XmlDocument> documents = new Dictionary<string, XmlDocument>();
	/// <summary>
	/// XML node for every Def, mapped to their keys.
	/// </summary>
	internal readonly Dictionary<string, XmlNode> resources = new Dictionary<string, XmlNode>();
	/// <summary>
	/// Fully processed Def objects mapped to their keys.
	/// </summary>
	internal readonly Dictionary<string, Def> defs = new Dictionary<string, Def>();

	/// <summary>
	/// Initializes the service with set options.
	/// </summary>
	/// <param name="options"><see cref="DefServiceOptions"/> for configuration.</param>
	public DefService(DefServiceOptions options) {
		this.options = options;
	}

	/// <summary>
	/// Registers an external directory containing XML files to the registry.
	/// </summary>
	/// <param name="directory">Full directory path containing the XML files.</param>
	/// <param name="errors">If the resource could not be registered, this will contain details about the errors that occurred.</param>
	/// <returns>True if all files were successfully registered; false if any were not registered.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the directory path is an empty string.</exception>
	/// <exception cref="DirectoryNotFoundException">Thrown if the directory path does not exist.</exception>
	public bool RegisterResource(string directory, [NotNullWhen(false)] out StringBuilder? errors) {
		errors = null;

		if (string.IsNullOrEmpty(directory)) {
			throw new ArgumentNullException(nameof(directory));
		}
		if (!Directory.Exists(directory)) {
			throw new DirectoryNotFoundException(directory);
		}

		IEnumerable<string> defFiles = XmlLoader.GetAllFiles(directory);
		List<string> failedFiles = new List<string>();
		foreach (string file in defFiles) {
			XmlDocument doc = XmlLoader.LoadDocument(file);
			if (!RegisterResource(file, doc)) {
				failedFiles.Add(file);
			}
		}

		if (failedFiles.Count != 0) {
			errors = new StringBuilder($"The following files could not be added: {string.Join(", ", failedFiles.Order())}");
			return false;
		}

		return true;
	}
	/// <summary>
	/// Registers XML files embedded in an assembly to the registry.
	/// </summary>
	/// <param name="assembly">Assembly containing the embedded XML files.</param>
	/// <param name="errors">If the resource could not be registered, this will contain details about the errors that occurred.</param>
	/// <returns>True if all files were successfully registered; false if any were not registered.</returns>
	public bool RegisterResource(Assembly assembly, [NotNullWhen(false)] out StringBuilder? errors) {
		errors = null;

		IEnumerable<string> defFiles = ResourceLoader.FetchResources(assembly, ".xml");
		if (!defFiles.Any()) {
			errors = new StringBuilder($"No resources were found in assembly '{assembly.GetName().Name}'");
			return false;
		}

		List<string> failedFiles = new List<string>();
		foreach (string file in defFiles) {
			Stream stream = ResourceLoader.LoadResourceStream(assembly, file);
			XmlDocument? doc = XmlLoader.LoadDocument(stream);
			if (doc == null) {
				errors = new StringBuilder($"Failed to parse XML document '{file}' from assembly '{assembly.GetName().Name}'");
				return false;
			}

			if (!RegisterResource($"{assembly.GetName().Name}.{file}", doc)) {
				failedFiles.Add(file);
			}
		}

		if (failedFiles.Count > 0) {
			errors = new StringBuilder($"The following files could not be added: {string.Join(", ", failedFiles.Order())}");
			return false;
		}

		return true;
	}
	/// <summary>
	/// Registeres a single XML document to the registry.
	/// </summary>
	/// <param name="document">Document to register.</param>
	/// <param name="errors">If the resource could not be registered, this will contain details about the errors that occurred.</param>
	/// <returns>True if the document was successfully registered; false otherwise.</returns>
	public bool RegisterResource(XmlDocument document, [NotNullWhen(false)] out StringBuilder? errors) {
		errors = null;
		if (!RegisterResource(HashCode.Combine(document.InnerText).ToString(), document)) {
			string contentPreview = document.OuterXml[..Math.Min(256, document.OuterXml.Length)];
			errors = new StringBuilder($"The document could not be added as a document with the same key has already been registered.\nDocument content: {contentPreview}");
			return false;
		}
		return true;
	}

	private bool RegisterResource(string key, XmlDocument document) {
		return documents.TryAdd(key, document);
	}

	/// <summary>
	/// Cleans and reloads all registered resources.
	/// </summary>
	public void Reload() {
		resources.Clear();
		defs.Clear();

		foreach (XmlDocument doc in documents.Values) {
			ParseDocument(doc);
		}

		if (options.DeferredLoad) {
			return;
		}

		foreach (XmlNode node in resources.Values) {
			_ = InitDef(node);
		}
	}

	/// <summary>
	/// Gets all the Def nodes in a document.
	/// </summary>
	/// <param name="doc">Document to search.</param>
	private void ParseDocument(XmlDocument doc) {
		XmlNode? defsNode = doc.SelectSingleNode(Constants.DEFS_ROOT_NODE);
		// Skip files that don't contain defs.
		if (defsNode == null) {
			return;
		}

		// Create a map of all Def nodes in this document.
		List<(string, XmlNode)> nodes = new List<(string, XmlNode)>();
		foreach (XmlNode child in defsNode.ChildNodes) {
			// Skip comment nodes.
			if (child.NodeType == XmlNodeType.Comment) {
				continue;
			}

			if (child.GetChildValue<bool>(Constants.DEF_DISABLED_ELEMENT)) {
				continue;
			}

			// Throw an exception if the Def node doesn't define a key.
			XmlNode? keyNode = child.SelectSingleNode(Constants.DEF_KEY_ELEMENT);
			if (keyNode == null) {
				throw new NodeMissingChildException(child, Constants.DEF_KEY_ELEMENT);
			}

			nodes.Add((keyNode.InnerText, child));
		}

		// Add each found node to the resources registry.
		foreach ((string key, XmlNode node) in nodes) {
			RegisterNode(key, node);
		}
	}
	private void RegisterNode(string key, XmlNode node) {
		if (!DefXMLUtils.ValidateDefName(key)) {
			throw new FormatException($"{Constants.DEF_KEY_ELEMENT} ('{key}') must be a valid identifier: '^[a-zA-Z@][a-zA-Z0-9\\-_]*$'.");
		}

		if (resources.TryGetValue(key, out XmlNode? baseNode)) {
			DefXMLUtils.InheritDefXML(ref baseNode, node);
		}
		else {
			resources.Add(key, node);
		}
	}

	/// <summary>
	/// Loads all registered Defs matching the specified type. Matches the type exactly.
	/// </summary>
	/// <typeparam name="T">Type of Defs to load.</typeparam>
	/// <returns>Collection of all Defs matching the supplied type.</returns>
	public IEnumerable<T> LoadAll<T>() where T : Def {
		IEnumerable<T> found = defs.Values
			.Where(d => d.GetType().Equals(typeof(T)))
			.Select(d => (T)d);

		if (!options.DeferredLoad) {
			return found;
		}

		// If deferred loading is enabled, also check every unloaded def
		// to see if it's the requested type.
		foreach ((string key, XmlNode node) in resources) {
			string className = node.GetAttributeValue(Constants.DEF_CLASS_ATTR);
			if (string.IsNullOrEmpty(className)) {
				continue;
			}
			// Check the Def's type against the requested type.
			if (className.Equals(typeof(T).ToString())) {
				// Load it!
				if (TryLoadDef(key, out T? loadedDef)) {
					_ = found.Append(loadedDef);
				}
			}
		}

		return found;
	}
	/// <summary>
	/// Loads all registered Defs.
	/// </summary>
	/// <returns>Collection of all Defs registered in the service.</returns>
	public IEnumerable<Def> LoadAll() {
		if (options.DeferredLoad) {
			foreach ((_, XmlNode node) in resources) {
				_ = InitDef(node);
			}
		}
		return defs.Values;
	}

	/// <summary>
	/// Attempts to load a Def object from the registry.
	/// </summary>
	/// <param name="key">Def key to load.</param>
	/// <param name="def">The stored Def object.</param>
	/// <typeparam name="T">Type of the Def to load.</typeparam>
	/// <returns>True if the Def could be loaded, false otherwise.</returns>
	public bool TryLoadDef<T>(string key, [NotNullWhen(true)] out T? def) where T : Def {
		if (TryLoadDef(key, out Def? value) && value is T typedDef) {
			def = typedDef;
			return true;
		}
		def = null;
		return false;
	}
	private bool TryLoadDef(string key, [NotNullWhen(true)] out Def? def) {
		try {
			def = LoadDef(key);
			return true;
		}
		catch (DefNotFoundException) {
			def = null;
			return false;
		}
	}

	/// <summary>
	/// Loads a Def object from the registry.
	/// </summary>
	/// <param name="key">Def key to load.</param>
	/// <typeparam name="T">Type of the Def to load.</typeparam>
	/// <returns>The stored Def object, or null if the Def exists but does not match the supplied type.</returns>
	/// <exception cref="DefNotFoundException">Thrown when a Def with the specified key could not be found.</exception>
	public T? LoadDef<T>(string key) where T : Def {
		if (LoadDef(key) is T typed) {
			return typed;
		}
		return null;
	}
	private Def LoadDef(string key) {
		if (defs.TryGetValue(key, out Def? def)) {
			return def;
		}

		if (resources.TryGetValue(key, out XmlNode? node)) {
			return InitDef(node);
		}

		throw new DefNotFoundException(key);
	}

	/// <summary>
	/// Parse a Def from XML and add it to the collection of parsed defs, removing it's unprocessed XML node.
	/// </summary>
	/// <param name="node">Def node to parse.</param>
	/// <returns>The parsed Def object.</returns>
	private Def InitDef(XmlNode node) {
		// Save a temporary version of the Def to load in case of circular references.
		defs.Add(DefXMLUtils.GetDefKey(node), DefXMLUtils.CreateTempDef(node));

		IEnumerable<Def> loadedDefs = this.ParseDef(node);
		foreach (Def def in loadedDefs) {
			// Skip temporary defs.
			if (def.IsTempDef()) {
				continue;
			}

			// Because a temporary instance is stored when initially loading a Def,
			// the 'defs' dictionary will always contains the key.
			// That means we can just assume it's already there, and we just need
			// to check if it's the temporary instance to overwrite it.
			if (defs[def.Key].IsTempDef()) {
				// Replace temporary entry.
				Def tempEntry = defs[def.Key];
				def.CopyTo(ref tempEntry);
			}

			_ = resources.Remove(def.Key);
		}
		return loadedDefs.First();
	}
}
