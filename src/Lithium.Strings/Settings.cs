using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Lithium.Core;

namespace Lithium.Strings;

/// <summary>
/// Settings used to determine from where and which strings to load.
/// </summary>
public static class Settings {
	/// <summary>
	/// The primary locale to use for string translations. This is the default locale that will be used if no other locale is set.
	/// </summary>
	public static CultureInfo PrimaryLocale { get; set; } = new CultureInfo("en-US");
	/// <summary>
	/// The current locale used for string translations. This determines which Fluent resource files are loaded and used for translating strings.
	/// </summary>
	public static CultureInfo Locale { get; private set; } = PrimaryLocale;
	/// <summary>
	/// A set of root directories to scan for Fluent resource files (.ftl) when loading string contexts.
	/// </summary>
	internal static HashSet<string> StringRootDirectories { get; private set; } = new HashSet<string>();
	/// <summary>
	/// Set of assemblies and their embedded Fluent resources to scan when loading string contents.
	/// </summary>
	internal static Dictionary<Assembly, HashSet<string>> EmbeddedResources { get; private set; } = new Dictionary<Assembly, HashSet<string>>();
	/// <summary>
	/// Evaluates whether any data sources have been added, either external or embedded.
	/// </summary>
	internal static bool HasData => StringRootDirectories.Count > 0 || EmbeddedResources.Count > 0;

	/// <summary>
	/// Char delimiter for portions of a string address.
	/// </summary>
	internal const char STRING_NAMESPACE_SEPARATOR = '.';

	/// <summary>
	/// Sets the current locale for string translations.
	/// This will trigger a reload of the string contexts to ensure that the appropriate Fluent resource files are loaded for the new locale.
	/// </summary>
	/// <param name="locale"></param>
	public static void SetLocale(string locale) {
		Locale = new CultureInfo(locale);

		if (HasData) {
			TranslationService.Reload();
		}
	}
	/// <summary>
	/// Adds a root directory to scan for Fluent resource files (.ftl) when loading string contexts.
	/// The directory should contain subdirectories named after locales (e.g. "en-US", "fr-FR") which in turn contain the .ftl files.
	/// </summary>
	/// <param name="path"></param>
	/// <exception cref="ArgumentNullException">Thrown when the provided path is null or empty.</exception>
	/// <exception cref="DirectoryNotFoundException">Thrown when the provided path does not exist.</exception>
	public static void AddStringRootDirectory(string path) {
		if (string.IsNullOrEmpty(path)) {
			throw new ArgumentNullException(nameof(path));
		}
		if (!Directory.Exists(path)) {
			throw new DirectoryNotFoundException(path);
		}

		_ = StringRootDirectories.Add(path);
	}

	/// <summary>
	/// Adds .ftl files embedded into an assembly as string files.
	/// </summary>
	/// <param name="assembly">Assembly containing the embedded resource files.</param>
	/// <remarks>
	/// The logical names of the resource files are expected to be in a particular format, identical to a hierarchical folder structure.
	/// e.g. 'root/locale/path/to/resource.ftl'.
	///
	/// Resources should be embedded as follows:
	///
	/// <code>
	///	&lt;EmbeddedResource Include=".../resources/MyStrings/**/*.ftl"&gt;
	///		&lt;LogicalName&gt;MyStrings/%(RecursiveDir)%(Filename)%(Extension)&lt;/LogicalName&gt;
	///	&lt;/EmbeddedResource&gt;
	/// </code>
	/// </remarks>
	/// <exception cref="ArgumentException">The assembly has already been added as a source.</exception>
	public static void AddEmbeddedResources(Assembly assembly) {
		IEnumerable<string> resources = ResourceLoader.FetchResources(assembly, ".ftl");
		if (!resources.Any()) {
			return;
		}
		if (EmbeddedResources.TryGetValue(assembly, out _)) {
			throw new ArgumentException($"Assembly already added: {assembly.GetName()}");
		}
		EmbeddedResources.Add(assembly, resources.ToHashSet());
	}

	/// <summary>
	/// Resets the settings to their default values.
	/// </summary>
	public static void Reset() {
		Locale = PrimaryLocale;
		StringRootDirectories.Clear();
		EmbeddedResources.Clear();
	}
}
