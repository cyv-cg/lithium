using System;
using System.Xml;
using Lithium.Core.Attributes;
using Lithium.Core;
using System.Diagnostics.CodeAnalysis;

namespace Lithium.Strings;

using StringArgument = (string key, object value);

/// <summary>
/// Represents a string that can be translated using the string database.
/// The string is identified by its namespace and key, which are used to look up the corresponding translation in the string database.
/// </summary>
/// <remarks>
/// This does not automatically ensure that the string it points to exists.
/// Use the IsLoaded method to check if the string key exists before attempting to translate it.
/// </remarks>
[UseDefOverrideInitializer]
public class KeyedString {
	/// <summary>
	/// Namespace of the string, used for lookup in the string context.
	/// </summary>
	public required string Namespace { get; init; }
	/// <summary>
	/// String key.
	/// </summary>
	public required string Key { get; init; }

	/// <summary>
	/// The full string address, combining the namespace and key.
	/// Used for translation lookups in the string context.
	/// </summary>
	public string Address => string.IsNullOrEmpty(Namespace) ? Key : $"{Namespace}.{Key}";

	/// <summary>
	/// Creates a KeyedString from a string address.
	/// </summary>
	/// <param name="address">String address in the format "root.namespace.category.string-name".</param>
	[SetsRequiredMembers]
	private KeyedString(string address) {
		(string @namespace, string key) = StringManager.ParseAddress(address);
		Namespace = @namespace;
		Key = key;
	}

	/// <summary>
	/// Creates a KeyedString from an XML node.
	/// Used when loading defs from XML.
	/// </summary>
	/// <param name="node">XML node for the string definition.</param>
	/// <returns>Loaded KeyedString value.</returns>
	[DefFactory]
	public static KeyedString Factory(XmlNode node) {
		return new KeyedString(node.InnerText);
	}

	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="values">String parameters.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	public string Translate(params StringArgument[] values) {
		return Address.Translate(values);
	}

	/// <summary>
	/// Implicitly converts a KeyedString to a string by translating it. This allows you to use a KeyedString directly in places where a string is expected, and it will automatically be translated using the string database.
	/// </summary>
	/// <remarks>
	/// This translates with no parameters. If the string has parameters, call the Translate method directly.
	/// </remarks>
	public static implicit operator string(KeyedString keyedString) {
		return keyedString.Translate();
	}
	/// <summary>
	/// Explicitly converts a string address to a KeyedString.
	/// This allows you to easily create a KeyedString from a string address when you know the address at compile time.
	/// </summary>
	/// <remarks>
	/// This does not validate that the string address exists in the string database. Use the IsLoaded method to check if the string key exists before attempting to translate it.
	/// </remarks>
	public static explicit operator KeyedString(string address) {
		return new KeyedString(address);
	}

	/// <summary>
	/// Checks whether the string key exists.
	/// </summary>
	/// <returns>True if the string key; otherwise, false.</returns>
	public bool IsLoaded() {
		return StringManager.TryGetMessage(Address, out _, out _);
	}

	/// <summary>
	/// Returns the translated string.
	/// This is equivalent to calling the Translate method with no parameters.
	/// If the string has parameters, call the Translate method directly.
	/// </summary>
	public override string ToString() {
		return (string)this;
	}
	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="values">String parameters.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	public string ToString(params StringArgument[] values) {
		return Translate(values);
	}

	public override bool Equals(object? obj) {
		if (obj == null) {
			return false;
		}

		if (obj is KeyedString otherKeyedString) {
			return otherKeyedString.GetHashCode() == GetHashCode();
		}
		else if (obj is string otherString) {
			return otherString == Address;
		}

		return false;
	}

	public override int GetHashCode() {
		return HashCode.Combine(Address);
	}
}
