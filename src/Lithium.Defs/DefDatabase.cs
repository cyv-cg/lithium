using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Lithium.Core;

namespace Lithium.Defs;

public static class DefDatabase {
	private static Dictionary<string, XmlNode>? XmlDefinitions { get; set; }
	private static Dictionary<string, Def>? ParsedDefinitions { get; set; }

	internal static void Initialize(IEnumerable<string> defFiles) {
		XmlDefinitions = new Dictionary<string, XmlNode>();

		foreach (string path in defFiles) {
			HashSet<Def> defs = new HashSet<Def>();

			XmlDocument doc = XmlLoader.LoadDocument(path);

			XmlNode? defsNode = doc.SelectSingleNode("/Defs");
			if (defsNode == null) {
				continue;
			}
			foreach (XmlNode child in defsNode.ChildNodes) {
				if (child.NodeType == XmlNodeType.Comment) {
					continue;
				}

				AddToDB(child);
			}
		}

		ParsedDefinitions = new Dictionary<string, Def>();
	}

	internal static IEnumerable<XmlNode> GetAllNodes() {
		if (XmlDefinitions == null) {
			throw new Exception("Nodes have not been initialized.");
		}
		return XmlDefinitions.Values;
	}

	private static void AddToDB(XmlNode node) {
		if (XmlDefinitions == null) {
			XmlDefinitions = new Dictionary<string, XmlNode>();
		}
		XmlDefinitions.TryAdd(GetDefKey(node), node);
	}
	internal static void AddToDB(Def def) {
		if (ParsedDefinitions == null) {
			ParsedDefinitions = new Dictionary<string, Def>();
		}
		ParsedDefinitions.TryAdd(def.key, def);
	}

	internal static string GetDefKey(XmlNode node) {
		XmlNode? keyNode = node.SelectSingleNode("key");
		if (keyNode == null) {
			throw new Exception("Def node missing 'key' child element.");
		}
		return keyNode.InnerText;
	}

	public static XmlNode LoadXml(string key) {
		if (XmlDefinitions == null) {
			throw new Exception("Defs have not been initialized.");
		}
		if (!XmlDefinitions.TryGetValue(key, out XmlNode? value)) {
			throw new Exception($"No Def was found with the key '{key}'.");
		}
		return value;
	}
	public static T? Load<T>(string key) where T : Def {
		if (ParsedDefinitions == null) {
			return null;
		}
		if (!ParsedDefinitions.TryGetValue(key, out Def? value)) {
			return null;
		}
		if (value is not T) {
			return null;
		}
		return value as T;
	}
}
