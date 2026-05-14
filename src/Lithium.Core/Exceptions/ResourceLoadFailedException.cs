using System;

namespace Lithium.Core.Exceptions;

/// <summary>
/// Exception indicating a resource failed to load.
/// </summary>
/// <param name="key">Key for the resource that failed.</param>
public class ResourceLoadFailedException(string key) : Exception {
	private readonly string key = key;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"Failed to load resource '{key}'.";
}
