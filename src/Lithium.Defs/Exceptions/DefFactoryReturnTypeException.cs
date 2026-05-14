using System;

namespace Lithium.Defs.Exceptions;

public class DefFactoryReturnTypeException(Type defType, Type? returnType = null) : Exception {
	private readonly Type defType = defType;
	private readonly Type? returnType = returnType;

	public override string Message {
		get {
			if (returnType != null) {
				return $"Def factory must return {defType} but it returns {returnType}.";
			}
			else {
				return $"Def factory must return {defType}.";
			}
		}
	}
}
