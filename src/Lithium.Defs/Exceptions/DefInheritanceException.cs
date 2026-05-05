using System;
using Lithium.Defs;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception thrown when a type that is expected to be a def does not inherit from Def.
/// </summary>
public class DefInheritanceException(Type type) : Exception {
	private readonly Type type = type;

	public override string Message => $"{type} must inherit {typeof(Def)}";
}
