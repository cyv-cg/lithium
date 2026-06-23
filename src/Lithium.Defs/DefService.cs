
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;

namespace Lithium.Defs;

public class DefService : IDefService, IResourceRegistry<string>, IResourceRegistry<Assembly>, IResourceRegistry<XmlDocument> {
	private readonly DefServiceOptions options;

	private readonly HashSet<XmlDocument> documents = new HashSet<XmlDocument>();
	internal readonly Dictionary<string, XmlNode> resources = new Dictionary<string, XmlNode>();
	internal readonly Dictionary<string, Def> defs = new Dictionary<string, Def>();

	public DefService(DefServiceOptions options) {
		this.options = options;
	}

	public bool RegisterResource(string directory) {
		if (string.IsNullOrEmpty(directory)) {
			throw new ArgumentNullException(nameof(directory));
		}
		if (!Directory.Exists(directory)) {
			throw new DirectoryNotFoundException(directory);
		}

		IEnumerable<string> defFiles = XmlLoader.GetAllFiles(directory);
		foreach (string file in defFiles) {
			XmlDocument doc = XmlLoader.LoadDocument(file);
			if (!RegisterResource(doc)) {
				return false;
			}
		}

		return true;
	}
	public bool RegisterResource(Assembly assembly) {
		IEnumerable<string> defFiles = ResourceLoader.FetchResources(assembly, ".xml");
		if (!defFiles.Any()) {
			return false;
		}

		foreach (string file in defFiles) {
			Stream? stream = ResourceLoader.LoadResourceStream(assembly, file);
			if (stream == null) {
				return false;
			}
			XmlDocument? doc = XmlLoader.LoadDocument(stream);
			if (doc == null) {
				continue;
			}

			if (!RegisterResource(doc)) {
				return false;
			}
		}

		return true;
	}
	public bool RegisterResource(XmlDocument document) {
		return documents.Add(document);
	}

	public void Reload() {
		resources.Clear();
		defs.Clear();

		foreach (XmlDocument doc in documents) {
			ParseDocument(doc);
		}

		if (options.DeferredLoad) {
			return;
		}

		foreach (XmlNode node in resources.Values) {
			_ = InitDef(node);
		}
	}

	private void ParseDocument(XmlDocument doc) {
		XmlNode? defsNode = doc.SelectSingleNode(Constants.DEFS_ROOT_NODE);
		// Skip files that don't contain defs.
		if (defsNode == null) {
			return;
		}

		List<(string, XmlNode)> nodes = new List<(string, XmlNode)>();
		foreach (XmlNode child in defsNode.ChildNodes) {
			// Skip comment nodes.
			if (child.NodeType == XmlNodeType.Comment) {
				continue;
			}

			XmlNode? keyNode = child.SelectSingleNode(Constants.DEF_KEY_ELEMENT);
			if (keyNode == null) {
				throw new NodeMissingChildException(child, Constants.DEF_KEY_ELEMENT);
			}

			nodes.Add((keyNode.InnerText, child));
		}

		foreach ((string key, XmlNode node) in nodes) {
			if (!resources.TryAdd(key, node)) {
				continue;
			}
		}
	}

	public IEnumerable<T> LoadAll<T>() where T : Def {
		HashSet<T> found = new HashSet<T>();

		foreach ((_, Def def) in defs) {
			if (def is T typed) {
				_ = found.Add(typed);
			}
		}

		if (!options.DeferredLoad) {
			return found;
		}

		// If deferred loading is enabled, also check every unloaded def
		// to see if it's the requested type.
		foreach ((string key, XmlNode node) in resources) {
			// Find what type the def is.
			Type? type = TypeChecker.ResolveType(node.Name);
			// Check against the requested type.
			if (type != null && type.Equals(typeof(T))) {
				// Load it!
				if (TryLoadDef(key, out T? loadedDef)) {
					_ = found.Add(loadedDef);
				}
			}
		}

		return found;
	}

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

	private Def InitDef(XmlNode node) {
		IEnumerable<Def> loadedDefs = this.ParseDef(node);
		foreach (Def def in loadedDefs) {
			if (defs.ContainsKey(def.Key)) {
				continue;
			}

			defs.Add(def.Key, def);
			_ = resources.Remove(def.Key);
		}
		return loadedDefs.First();
	}
}
