using System;
using Lithium.Strings;
using Lithium.Strings.Exceptions;

using StringArgument = (string key, object value);

namespace Lithium.Defs;

/// <summary>
/// Root structure for all Defs.
/// </summary>
public class Def {
	/// <summary>
	/// Primary key used to solely define the object.
	/// Must be distinct from all other Defs.
	/// </summary>
	public required string Key { get; init; }
	/// <summary>
	/// String name for the Def.
	/// </summary>
	public required KeyedString Label { get; init; }
	/// <summary>
	/// Whether or not the Def should be treated as not in-use.
	/// </summary>
	public bool Disabled { get; init; } = false;

	/// <summary>
	/// Translates the label by replacing parameters with the given values.
	/// If the string has not been loaded into the context, returns the string address instead.
	/// </summary>
	/// <param name="values">String parameters.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	/// <exception cref="StringTranslationException">Thrown when there is an error during translation or interpolation.</exception>
	/// <exception cref="ArgumentException">Thrown when an argument key is null or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	public string ToString(params StringArgument[] values) {
		return Label.Translate(values);
	}

	/// <summary>
	/// Implicitly converts a Def to a string by translating its label.
	/// </summary>
	/// <remarks>
	/// This translates with no parameters. If the string has parameters, call the Translate method directly.
	/// </remarks>
	/// <exception cref="StringTranslationException">Thrown when there is an error during translation or interpolation.</exception>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	public static implicit operator string(Def def) {
		return def.ToString();
	}

	/// <inheritdoc/>
	public virtual bool Equals(Def? other) {
		if (other == null) {
			return false;
		}
		return GetHashCode() == other.GetHashCode();
	}
	/// <inheritdoc/>
	public override int GetHashCode() {
		return HashCode.Combine(Key);
	}
}
