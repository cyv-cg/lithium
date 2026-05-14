using System;

namespace Lithium.Core.Exceptions;

/// <summary>
/// Exception indicating a type could not be resolved from a name.
/// </summary>
/// <param name="name">String that failed to resolve to a type.</param>
public class UnresolvedTypeException(string name) : Exception {
	private readonly string name = name;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"Could not resolve '{name}' to a type.";
}
