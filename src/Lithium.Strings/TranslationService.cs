using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Fluent.Net;
using Fluent.Net.RuntimeAst;
using Lithium.Core;
using Lithium.Strings.Exceptions;

using StringArgument = (string key, object value);

namespace Lithium.Strings;

/// <summary>
/// Utilities for translating strings to the current locale.
/// </summary>
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
	/// <exception cref="StringTranslationException">Thrown when there is an error during translation or interpolation.</exception>
	/// <exception cref="ArgumentException">Thrown when an argument key is null or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	public static string Translate(this string key, params StringArgument[] args) {
		if (!StringManager.TryGetMessage(key, out MessageContext? context, out Message? message)) {
			throw new KeyNotFoundException(key);
		}

		// Translate and interpolate.
		List<FluentError> errors = new List<FluentError>();
		string result = context!.Format(message, FormatArgs(args), errors);
		// Re-throw the errors.
		if (errors.Count != 0) {
			throw new StringTranslationException(errors);
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

	/// <summary>
	/// Calculates the translation completion rate for each locale based on the number of string keys that have been translated compared to the primary locale.
	/// </summary>
	/// <returns>A dictionary mapping locale names to their corresponding translation completion rates (between 0 and 1).</returns>
	public static Dictionary<string, float> CalculateTranslationCompletion() {
		Dictionary<string, float> rates = new Dictionary<string, float> {
			[Settings.PrimaryLocale.Name] = 1f
		};

		if (!Settings.HasData) {
			return rates;
		}

		string[] primaryLocaleStrings = GetAllKeysForLocale(Settings.PrimaryLocale.Name).ToArray();

		foreach (string root in Settings.StringRootDirectories) {
			// Get the name of every available locale from the directory names.
			string[] locales = Directory.GetDirectories(root).Select(d => Path.GetFileName(d)).ToArray();

			foreach (string locale in locales) {
				// The primary locale is always assumed to be complete, so skip counting it.
				if (locale.Equals(Settings.PrimaryLocale.Name)) {
					continue;
				}
				// Get all the strings for the secondary locale.
				IEnumerable<string> localeStrings = GetAllKeysForLocale(locale);
				// Count the number of strings from the primary locale which are also defined for the secondary locale.
				// This explicitly does not count strings defined in the secondary locale but not in the primary.
				uint numerator = (uint)primaryLocaleStrings.Count(k => localeStrings.Contains(k));
				rates[locale] = (float)numerator / primaryLocaleStrings.Length;
			}
		}

		return rates;
	}
	/// <summary>
	/// Helper function to get the name of every string in a given locale.
	/// </summary>
	/// <param name="locale">Name of the locale.</param>
	/// <returns>A collection of all string addresses defined in the given locale.</returns>
	private static HashSet<string> GetAllKeysForLocale(string locale) {
		HashSet<string> addresses = new HashSet<string>();

		// Count resources from embedded files.
		foreach (Assembly assembly in Settings.EmbeddedResources.Keys) {
			IEnumerable<string> resources = Settings.EmbeddedResources[assembly];
			foreach (string resource in resources) {
				(string _, string resourceLocale, string _) = StringManager.ParseEmbeddedResourceName(resource);
				if (!resourceLocale.Equals(locale)) {
					continue;
				}
				Stream? stream = ResourceLoader.LoadResourceStream(assembly, resource);
				if (stream == null) {
					continue;
				}
				StreamReader reader = new StreamReader(stream);
				addresses.UnionWith(LoadEntries(reader, StringManager.GetNamespace(resource)));
			}
		}

		foreach (string directory in Settings.StringRootDirectories) {
			// Get every Fluent resource file for the locale.
			string localeDirectory = Path.Combine(directory, locale);
			if (!Directory.Exists(localeDirectory)) {
				continue;
			}
			string[] files = Directory.GetFiles(localeDirectory, "*.ftl", SearchOption.AllDirectories);

			foreach (string file in files) {
				// Parse the file into a resource.
				StreamReader reader = new StreamReader(file);
				addresses.UnionWith(LoadEntries(reader, StringManager.GetNamespace(directory, locale, file)));
			}
		}

		return addresses;
	}

	private static IEnumerable<string> LoadEntries(StreamReader reader, string @namespace) {
		FluentResource resource = FluentResource.FromReader(reader);
		// Fetch and store each string key.
		IEnumerable<string> entries = resource.Entries.Select(e => $"{@namespace}.{e.Key}");
		return entries;
	}
}
