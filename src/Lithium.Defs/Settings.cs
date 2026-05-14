using System;
using System.Collections.Generic;
using System.IO;

namespace Lithium.Defs;

/// <summary>
/// Settings for where and how to load Defs.
/// </summary>
public static class Settings {
	/// <summary>
	/// Root directory from which to start recursively checking for XML definition files.
	/// </summary>
	public static HashSet<string>? DefRootDirectories { get; private set; }
	/// <summary>
	/// Deferred Parsing will wait for a def to be used before parsing it from XML.
	/// Non-Deferred Parsing will immediately parse all defs at startup.
	/// </summary>
	public static bool DeferredParsing { get; set; } = true;

	/// <summary>
	/// Adds a root directory to scan for Fluent resource files (.ftl) when loading string contexts.
	/// The directory should contain subdirectories named after locales (e.g. "en-US", "fr-FR") which in turn contain the .ftl files.
	/// </summary>
	/// <param name="path"></param>
	/// <exception cref="ArgumentNullException">Thrown when the provided path is null or empty.</exception>
	/// <exception cref="DirectoryNotFoundException">Thrown when the provided path does not exist.</exception>
	public static void AddDefRootDirectory(string path) {
		if (string.IsNullOrEmpty(path)) {
			throw new ArgumentNullException(nameof(path));
		}
		if (!Directory.Exists(path)) {
			throw new DirectoryNotFoundException(path);
		}

		DefRootDirectories ??= new HashSet<string>();

		_ = DefRootDirectories.Add(path);
	}
}
