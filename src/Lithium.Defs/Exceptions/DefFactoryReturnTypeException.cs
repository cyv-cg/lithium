using System;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception indicating a def factory has an invalid return type.
/// </summary>
public class DefFactoryReturnTypeException(Type defType, Type? returnType = null) : Exception {
	private readonly Type defType = defType;
	private readonly Type? returnType = returnType;

	/// <summary>
	/// Message describing the error.
	/// </summary>
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
