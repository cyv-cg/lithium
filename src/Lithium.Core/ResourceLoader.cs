using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lithium.Core;

public static class ResourceLoader {
	public static Stream LoadResourceStream(Assembly assembly, string resource) {
		Stream? stream = assembly.GetManifestResourceStream(resource);

		if (stream == null) {
			throw new Exception();
		}

		return stream;
	}

	public static IEnumerable<string> FetchResources(Assembly assembly, string? extension = null) {
		string[] resources = assembly.GetManifestResourceNames();

		if (string.IsNullOrEmpty(extension)) {
			return resources;
		}

		return resources.Where(r => Path.GetExtension(r).Equals(extension));
	}
}
