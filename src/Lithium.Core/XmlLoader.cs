using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace Lithium.Core;

/// <summary>
/// Utility class for loading XML documents from files.
/// </summary>
public static class XmlLoader {
	/// <summary>
	/// Recursively scans the directory and returns all XML files found.
	/// </summary>
	/// <param name="root">Root directory to start scanning from.</param>
	/// <returns>Collection of file paths for all XML files found.</returns>
	/// <exception cref="DirectoryNotFoundException">Thrown if the specified root directory does not exist.</exception>
	public static IEnumerable<string> GetAllFiles(string root) {
		if (!Directory.Exists(root)) {
			throw new DirectoryNotFoundException($"Directory '{root}' does not exist.");
		}

		return Directory.GetFiles(root, "*.xml", SearchOption.AllDirectories);
	}

	/// <summary>
	/// Loads an XML document from the specified file path. Throws exceptions if the file does not exist or is not a valid XML file.
	/// </summary>
	/// <param name="fileName">The path to the XML file to load.</param>
	/// <returns>The loaded <see cref="XmlDocument"/>.</returns>
	/// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
	/// <exception cref="FileLoadException">Thrown if the specified file is not a .xml file.</exception>
	/// <exception cref="XmlException">Thrown if the file contents cannot be parsed as valid XML.</exception>
	public static XmlDocument LoadDocument(string fileName) {
		if (!File.Exists(fileName)) {
			throw new FileNotFoundException($"File '{fileName}' does not exist.");
		}
		if (!Path.GetExtension(fileName).Equals(".xml")) {
			throw new FileLoadException($"'{fileName}' must be an XML file.");
		}
		string contents = File.ReadAllText(fileName);
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(contents);
		return doc;
	}
	/// <summary>
	/// Loads an XML document from a provided stream.
	/// </summary>
	/// <param name="stream">The <see cref="Stream"/> to read the XML content from.</param>
	/// <returns>The loaded <see cref="XmlDocument"/> if the stream contains valid XML; otherwise, <c>null</c>.</returns>
	/// <exception cref="XmlException">Thrown if the file contents cannot be parsed as valid XML.</exception>
	/// <exception cref="ArgumentException">Stream does not support reading.</exception>
	/// <exception cref="ArgumentNullException">Stream is null.</exception>
	public static XmlDocument? LoadDocument(Stream stream) {
		StreamReader reader = new StreamReader(stream);
		string content = reader.ReadToEnd();

		if (string.IsNullOrEmpty(content)) {
			return null;
		}

		XmlDocument doc = new XmlDocument();
		doc.LoadXml(content);

		return doc;
	}

	/// <summary>
	/// Retrieves the value of a child XML node with the specified name and converts it to the given type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type to which the node value should be converted.</typeparam>
	/// <param name="parent">The parent <see cref="XmlNode"/> containing the child node.</param>
	/// <param name="nodeName">The name of the child node to retrieve.</param>
	/// <returns>
	/// The value of the child node converted to type <typeparamref name="T"/> if the node exists; otherwise, the default value of <typeparamref name="T"/>.
	/// </returns>
	public static T? GetChildValue<T>(this XmlNode parent, string nodeName) {
		XmlNode? keyNode = parent.SelectSingleNode(nodeName);
		if (keyNode != null) {
			return (T)Convert.ChangeType(keyNode.InnerText, typeof(T));
		}
		return default;
	}
	/// <summary>
	/// Retrieves the value of a child XML node with the specified name, converts it to the specified type <typeparamref name="T"/>, and outputs the child node.
	/// </summary>
	/// <typeparam name="T">The type to which the child node's inner text will be converted.</typeparam>
	/// <param name="parent">The parent <see cref="XmlNode"/> to search for the child node.</param>
	/// <param name="nodeName">The name of the child node to retrieve.</param>
	/// <param name="child">
	/// When this method returns, contains the <see cref="XmlNode"/> found with the specified name, or <c>null</c> if no such node exists.
	/// </param>
	/// <returns>
	/// The value of the child node's inner text converted to type <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> if the node is not found.
	/// </returns>
	public static T? GetChildValue<T>(this XmlNode parent, string nodeName, out XmlNode? child) {
		child = null;
		XmlNode? keyNode = parent.SelectSingleNode(nodeName);
		if (keyNode != null) {
			child = keyNode;
			return (T)Convert.ChangeType(keyNode.InnerText, typeof(T));
		}
		return default;
	}
	/// <summary>
	/// Retrieves the value of an attribute from the specified XML node. If the attribute does not exist, returns an empty string.
	/// </summary>
	/// <param name="parent">The <see cref="XmlNode"/> from which to retrieve the attribute value.</param>
	/// <param name="attribute">The name of the attribute whose value is to be retrieved.</param>
	/// <returns>The value of the specified attribute, or an empty string if the attribute does not exist.</returns>
	public static string GetAttributeValue(this XmlNode parent, string attribute) {
		if (parent.Attributes == null) {
			return string.Empty;
		}
		XmlAttribute? attr = parent.Attributes[attribute];
		if (attr == null) {
			return string.Empty;
		}
		return attr.Value;
	}
}
