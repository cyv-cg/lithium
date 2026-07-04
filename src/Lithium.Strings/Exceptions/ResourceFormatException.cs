using System;
using System.IO;

namespace Lithium.Strings.Exceptions;

/// <summary>
/// Exception thrown when a string resource path is in an improper format.
/// </summary>
public class ResourceFormatException(string resourcePath, bool embedded, string locale) : Exception {
	private readonly string resourcePath = resourcePath;
	private readonly bool embedded = embedded;
	private readonly string locale = locale;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message {
		get {
			char c = Path.DirectorySeparatorChar;
			if (embedded) {
				return $"{nameof(resourcePath)} must be in the format 'root{Constants.EMBEDDED_RESOURCE_ROOT_INDICATOR}{locale}{c}path{c}to{c}resource.ext': '{resourcePath}'.";
			}
			else {
				return $"{nameof(resourcePath)} must be in the format '...{c}root{c}{locale}{c}path{c}to{c}resource.ext': '{resourcePath}'.";
			}
		}
	}
}
