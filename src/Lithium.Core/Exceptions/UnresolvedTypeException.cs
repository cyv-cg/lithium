using System;

namespace Lithium.Core.Exceptions;

public class UnresolvedTypeException(string name) : Exception {
	private string name = name;

	public override string Message => $"Could not resolve '{name}' to a type.";
}
