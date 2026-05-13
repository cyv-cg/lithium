using System;
using System.Collections.Generic;
using System.Linq;
using Fluent.Net;
using Fluent.Net.RuntimeAst;

namespace Lithium.Strings;

using StringArgument = (string key, object value);

public static class TranslationService {
	/// <summary>
	/// Reloads the string contexts by scanning the root directories for Fluent resource files corresponding to the current locale.
	/// This should be called after adding new root directories to ensure that the latest string resources are loaded and available for translation.
	/// </summary>
	/// <remarks>
	/// Automatically called when changing the locale.
	/// </remarks>
	public static void Reload() {
		StringManager.Reload();
	}

	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="key">The key of the string to translate, including its namespace (e.g. root.namespace.category.string-key).</param>
	/// <param name="args">Tuples where the first item is the placeable name and the second is the value.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	/// <exception cref="KeyNotFoundException">Thrown when the provided key does not exist in the string database.</exception>
	/// <exception cref="Exception">Thrown when there is an error during translation or interpolation.</exception>
	public static string Translate(this string key, params StringArgument[] args) {
		if (!StringManager.TryGetMessage(key, out MessageContext? context, out Message? message)) {
			throw new KeyNotFoundException(key);
		}

		// Translate and interpolate.
		List<FluentError> errors = new List<FluentError>();
		string result = context!.Format(message, FormatArgs(args), errors);
		// Throw the first oopsie.
		if (errors.Count != 0) {
			throw new Exception(errors.First().Message);
		}

		return result;
	}

	/// <summary>
	/// Formats the provided arguments into a dictionary for Fluent interpolation.
	/// </summary>
	/// <param name="args">Tuples where the first item is the placeable name and the second is the value.</param>
	/// <returns>A dictionary mapping placeable names to their corresponding values.</returns>
	/// <exception cref="ArgumentException">Thrown when an argument key is null or empty.</exception>
	private static Dictionary<string, object> FormatArgs(params StringArgument[] args) {
		Dictionary<string, object> argsMap = new Dictionary<string, object>();

		for (int i = 0; i < args.Length; i++) {
			if (string.IsNullOrEmpty(args[i].key)) {
				throw new ArgumentException($"Expected the argument at index {i} to be a non-empty string", nameof(args));
			}
			argsMap.Add(args[i].key, args[i].value);
		}

		return argsMap;
	}
}
