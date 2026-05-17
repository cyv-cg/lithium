using System;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception indicating a def could not be located by the given key.
/// </summary>
/// <param name="key">Missing def key.</param>
public class DefNotFoundException(string key) : Exception {
	private readonly string key = key;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"No Def was found with the key '{key}'.";
}
