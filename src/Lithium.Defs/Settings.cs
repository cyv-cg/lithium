using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lithium.Core;

namespace Lithium.Defs;

/// <summary>
/// Settings for where and how to load Defs.
/// </summary>
public static class Settings {
	/// <summary>
	/// Root directory from which to start recursively checking for XML definition files.
	/// </summary>
	public static HashSet<string> DefRootDirectories { get; private set; } = new HashSet<string>();
	internal static Dictionary<Assembly, HashSet<string>> EmbeddedResources { get; private set; } = new Dictionary<Assembly, HashSet<string>>();
	/// <summary>
	/// Deferred Parsing will wait for a def to be used before parsing it from XML.
	/// Non-Deferred Parsing will immediately parse all defs at startup.
	/// </summary>
	public static bool DeferredParsing { get; set; } = true;

	/// <summary>
	/// Adds a root directory to scan for def resource XML files.
	/// </summary>
	/// <param name="path">Directory path for def files.</param>
	/// <exception cref="ArgumentNullException">Thrown when the provided path is null or empty.</exception>
	/// <exception cref="DirectoryNotFoundException">Thrown when the provided path does not exist.</exception>
	public static void AddDefRootDirectory(string path) {
		if (string.IsNullOrEmpty(path)) {
			throw new ArgumentNullException(nameof(path));
		}
		if (!Directory.Exists(path)) {
			throw new DirectoryNotFoundException(path);
		}

		_ = DefRootDirectories.Add(path);
	}

	public static void AddEmbeddedResources(Assembly assembly) {
		IEnumerable<string> resources = ResourceLoader.FetchResources(assembly, ".xml");
		if (!resources.Any()) {
			return;
		}

		if (EmbeddedResources.TryGetValue(assembly, out _)) {
			throw new Exception();
		}
		EmbeddedResources.Add(assembly, resources.ToHashSet());
	}
}
