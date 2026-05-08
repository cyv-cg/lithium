using System;

namespace Lithium.Defs.Exceptions;

public class DefFactoryReturnTypeException(Type defType, Type? returnType) : Exception {
	private readonly Type defType = defType;
	private readonly Type? returnType = returnType;

	public override string Message => $"Def factory must return {defType} but it returns {returnType}.";
}
