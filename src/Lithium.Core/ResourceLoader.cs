using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lithium.Core;

/// <summary>
/// Utilities for loading content from embedded resources.
/// </summary>
public static class ResourceLoader {
	/// <summary>
	/// Attempts to load a given embedded resource from an assembly.
	/// </summary>
	/// <param name="assembly">Assembly containing the resource.</param>
	/// <param name="resource">Resource name.</param>
	/// <returns>A <see cref="Stream"/> able to read the contents of the resource.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the <c>resources</c> parameter is null.</exception>
	/// <exception cref="ArgumentException">Thrown if the <c>resources</c> parameter is empty.</exception>
	/// <exception cref="FileLoadException">A file that was found could not be loaded.</exception>
	/// <exception cref="FileNotFoundException">The resource could not be found.</exception>
	/// <exception cref="BadImageFormatException"><c>assembly</c> is not a valid assembly.</exception>
	/// <exception cref="NotImplementedException">Resource length is greater than <c>Int64.MaxValue</c>.</exception>
	public static Stream LoadResourceStream(Assembly assembly, string resource) {
		try {
			Stream? stream = assembly.GetManifestResourceStream(resource);
			return stream!;
		}
		catch {
			throw;
		}
	}

	/// <summary>
	/// Fetches all resources embedded in an assembly.
	/// </summary>
	/// <param name="assembly">Assembly containing the resources.</param>
	/// <param name="extension">
	/// 	If passed, only matches resources with the extension (including the period ".").
	/// 	Otherwise gets all embedded resources.
	/// </param>
	/// <returns>List of resource names embedded in the assembly.</returns>
	public static IEnumerable<string> FetchResources(Assembly assembly, string? extension = null) {
		string[] resources = assembly.GetManifestResourceNames();

		if (string.IsNullOrEmpty(extension)) {
			return resources;
		}

		return resources.Where(r => Path.GetExtension(r).Equals(extension));
	}
}
