namespace Lithium.Defs;

public static class Settings {
	/// <summary>
	/// Root directory from which to start recursively checking for XML definition files.
	/// </summary>
	public static string? DefRootDirectory { get; private set; }
	/// <summary>
	/// Deferred Parsing will wait for a def to be used before parsing it from XML.
	/// Non-Deferred Parsing will immediately parse all defs at startup.
	/// </summary>
	public static bool DeferredParsing { get; set; } = true;

	public static void SetDefRootDirectory(string path) {
		DefRootDirectory = path;
	}
}
