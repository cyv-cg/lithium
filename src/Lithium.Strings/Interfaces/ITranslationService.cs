using System.Collections.Generic;

using StringArgument = (string key, object value);

namespace Lithium.Strings;

/// <summary>
/// Base interface for a service used to translate strings.
/// </summary>
public interface ITranslationService {
	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="key">The key of the string to translate.</param>
	/// <param name="args">Tuples where the first item is the placeable name and the second is the value.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	string Translate(string key, params StringArgument[] args);

	/// <summary>
	/// Fetches a list of all string keys in the service.
	/// </summary>
	/// <returns>List of string keys.</returns>
	IEnumerable<string> GetAllStringKeys();
	/// <summary>
	/// Determine whether a string with the given key is defined as a translatable unit.
	/// </summary>
	/// <param name="key">String key to search for.</param>
	/// <returns>True if the string is loaded.</returns>
	bool HasMessage(string key);
}
