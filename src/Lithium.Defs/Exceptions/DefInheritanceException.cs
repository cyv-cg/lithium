using System;
using Lithium.Defs;

namespace Lithium.Defs.Exceptions;

public class DefInheritanceException(Type type) : Exception
{
	private readonly Type type = type;

	public override string Message => $"{type} must inherit {typeof(Def)}";
}
