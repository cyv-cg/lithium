using System;
using Lithium.Core.Attributes;

namespace Lithium.Defs.Exceptions;

public class DefFactoryMissingException(Type defType) : Exception {
	private readonly Type defType = defType;

	public override string Message => $"{defType} has the {typeof(UseDefOverrideInitializer)} attribute but has no constructor with the {typeof(DefConstructor)} attribute or method with the {typeof(DefFactory)} attribute.";
}
