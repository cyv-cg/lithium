using System;
using Lithium.Strings;
using Lithium.Strings.Exceptions;

using StringArgument = (string key, object value);

namespace Lithium.Defs;

/// <summary>
/// Root structure for all Defs.
/// </summary>
public record Def {
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
	/// Translates the label with no parameters.
	/// If the string has not been loaded into the context, returns the string address instead.
	/// </summary>
	/// <returns>Translated string with parameters replaced.</returns>
	/// <exception cref="StringTranslationException">Thrown when there is an error during translation or interpolation.</exception>
	public override string ToString() {
		return Label.Translate();
	}
	/// <summary>
	/// Translates the label by replacing parameters with the given values.
	/// If the string has not been loaded into the context, returns the string address instead.
	/// </summary>
	/// <param name="values">String parameters.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	/// <exception cref="StringTranslationException">Thrown when there is an error during translation or interpolation.</exception>
	/// <exception cref="ArgumentException">Thrown when an argument key is null or empty.</exception>
	public string ToString(params StringArgument[] values) {
		return Label.Translate(values);
	}
}
