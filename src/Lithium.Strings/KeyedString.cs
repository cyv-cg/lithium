using System;
using System.Xml;
using Lithium.Core.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Lithium.Core;

namespace Lithium.Strings;

[UseDefOverrideInitializer]
public partial class KeyedString {
	/// <summary>
	/// String key.
	/// </summary>
	public string key;
	/// <summary>
	/// Raw text for the <see cref="KeyedString"/> as defined in XML .
	/// </summary>
	private string? raw;

	/// <summary>
	/// Creates a KeyedString from an XML node.
	/// </summary>
	/// <param name="key">Key of the defined string, used for XML lookup.</param>
	public KeyedString(string key) {
		XmlNode? stringNode = StringDatabase.LoadXml(key);
		if (stringNode == null) {
			throw new Exception($"String def '{key}' not found.");
		}

		this.key = StringDatabase.GetStringKey(stringNode);

		foreach (XmlNode node in stringNode) {
			raw = GetLocaleText(node);
		}

		StringDatabase.AddToDB(this);
	}
	/// <summary>
	/// Creates a KeyedString from an XML node.
	/// Used when loading defs from XML.
	/// </summary>
	/// <param name="node">XML node for the string definition.</param>
	/// <returns>Loaded KeyedString value.</returns>
	[DefFactory]
	public static KeyedString Factory(XmlNode node) {
		return StringDatabase.Load(node.InnerText);
	}

	/// <summary>
	/// Parses the locale text values from an XML node.
	/// </summary>
	/// <param name="node">XML node for the string definition.</param>
	/// <returns>Map from the locale code to the localized string value.</returns>
	private static string GetLocaleText(XmlNode node) {
		string key = StringDatabase.GetStringKey(node);
		XmlNode? textNode = node.SelectSingleNode("text");

		if (textNode == null) {
			throw new Exception($"KeyedString '{key}' has no text values.");
		}

		// Read "noTrim" node value and trim the raw text accordingly.
		bool noTrim = node.GetChildValue<bool>("noTrim");
		string? text = null;

		foreach (XmlNode localeText in textNode.ChildNodes) {
			if (localeText.InnerText.Equals(StringParser.Locale)) {
				text = noTrim ? localeText.InnerText : localeText.InnerText.Trim();
				break;
			}
		}

		if (text == null) {
			throw new Exception($"No text found for string '{key}'.");
		}

		return text;
	}

	/// <summary>
	/// Counts the number of parameters in the raw text.
	/// Parameters are in the form {0}, {1}, etc.
	/// </summary>
	/// <returns>Number of string parameters.</returns>
	private byte CountParams() {
		if (string.IsNullOrEmpty(raw)) {
			return 0;
		}
		return (byte)StringParamsPattern().Matches(raw).Count;
	}

	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="values">String parameters.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	public string Translate(params object[] values) {
		if (string.IsNullOrEmpty(raw)) {
			return string.Empty;
		}

		byte numParams = CountParams();
		byte numProvidedParams = (byte)(values != null ? values.Length : 0);

		if (numParams != numProvidedParams) {
			throw new WarningException($"String '{key}' takes in {numParams} parameter(s), but {numProvidedParams} were provided.");
		}

		string parsed = raw;

		for (byte i = 0; i < Math.Min(numProvidedParams, numParams); i++) {
			string pattern = @"\{" + i + @"\}";
			string value = values?[i].ToString() ?? string.Empty;
			parsed = Regex.Replace(parsed, pattern, value);
		}

		return parsed;
	}

	public static implicit operator string(KeyedString keyedString) {
		return keyedString.Translate();
	}
	public override string ToString() {
		return Translate();
	}

	[GeneratedRegex(@"\{\d\}")]
	private static partial Regex StringParamsPattern();
}
