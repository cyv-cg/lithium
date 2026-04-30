using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.ComponentModel;

namespace Lithium.Strings;

public static class StringDatabase {
	private static Dictionary<string, XmlNode>? XmlDefinitions { get; set; }
	private static Dictionary<string, KeyedString>? Keys { get; set; }

	internal static void Initialize(IEnumerable<string> stringFiles, string locale) {
		Keys = new Dictionary<string, KeyedString>();
		XmlDefinitions = new Dictionary<string, XmlNode>();

		foreach (string path in stringFiles) {
			string contents = File.ReadAllText(path);

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(contents);

			XmlNode? stringsNode = doc.SelectSingleNode("/Strings");
			if (stringsNode == null) {
				continue;
			}
			foreach (XmlNode entry in stringsNode.ChildNodes) {
				AddToDB(entry);
			}
		}
	}

	private static void AddToDB(XmlNode node) {
		if (XmlDefinitions == null) {
			XmlDefinitions = new Dictionary<string, XmlNode>();
		}
		string key = GetStringKey(node);
		if (XmlDefinitions.TryGetValue(key, out XmlNode? _)) {
			throw new WarningException($"Duplicate definition for string '{key}'.\n{node}");
		}
		else {
			XmlDefinitions.Add(key, node);
		}
	}
	internal static void AddToDB(KeyedString keyedString) {
		if (Keys == null) {
			Keys = new Dictionary<string, KeyedString>();
		}
		Keys.Add(keyedString.key, keyedString);
	}

	internal static string GetStringKey(XmlNode node) {
		XmlNode? keyNode = node.SelectSingleNode("key");
		if (keyNode == null) {
			throw new Exception("String node missing 'key' child element.");
		}
		return keyNode.InnerText;
	}

	internal static XmlNode? LoadXml(string key) {
		if (XmlDefinitions == null) {
			return null;
		}
		if (!XmlDefinitions.TryGetValue(key, out XmlNode? keyedString)) {
			throw new Exception($"No string was found with the key '{key}'.");
		}
		return keyedString;
	}
	public static KeyedString Load(string key) {
		if (Keys == null) {
			throw new Exception("Keys have not been loaded.");
		}
		if (!Keys.TryGetValue(key, out KeyedString? keyedString)) {
			keyedString = new KeyedString(key);
		}
		return keyedString;
	}
}
