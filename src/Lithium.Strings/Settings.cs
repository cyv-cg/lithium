using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
	internal static HashSet<string>? StringRootDirectories { get; private set; }

	/// <summary>
	/// Sets the current locale for string translations.
	/// This will trigger a reload of the string contexts to ensure that the appropriate Fluent resource files are loaded for the new locale.
	/// </summary>
	/// <param name="locale"></param>
	public static void SetLocale(string locale) {
		Locale = new CultureInfo(locale);

		if (StringRootDirectories != null) {
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

		StringRootDirectories ??= new HashSet<string>();

		_ = StringRootDirectories.Add(path);
	}

	/// <summary>
	/// Resets the settings to their default values.
	/// </summary>
	public static void Reset() {
		Locale = PrimaryLocale;
		StringRootDirectories = null;
	}
}
