using System.Collections.Generic;
using System.Reflection;

using StringArgument = (string key, object value);

namespace Lithium.Strings;

/// <summary>
/// Interface for a service used to translate strings.
/// </summary>
public interface ITranslationService {
	/// <summary>
	/// Convert registered string resources into usable translatable units.
	/// </summary>
	void Reload();
	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="key">The key of the string to translate.</param>
	/// <param name="args">Tuples where the first item is the placeable name and the second is the value.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	string Translate(string key, params StringArgument[] args);
	/// <summary>
	/// Fetches a list of all string addresses in the service.
	/// </summary>
	/// <returns>List of string addresses.</returns>
	IEnumerable<string> GetAllStringAddresses();

	/// <summary>
	/// Registers a collection of external string resources within a directory.
	/// </summary>
	/// <param name="directory">Path of the directory containing the string resources.</param>
	/// <returns>True if the resource was successfully registered.</returns>
	bool RegisterResource(string directory);
	/// <summary>
	/// Registers string resources embedded within an assembly.
	/// </summary>
	/// <param name="assembly">Assembly containing the string resources.</param>
	/// <returns>True if the resource was successfully registered.</returns>
	bool RegisterResource(Assembly assembly);
}
