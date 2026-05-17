using System;
using Lithium.Core.Attributes;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception indicating a def class is missing an expected factory method or constructor.
/// </summary>
public class DefFactoryMissingException(Type defType) : Exception {
	private readonly Type defType = defType;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"{defType} has the {typeof(UseDefOverrideInitializer)} attribute but has no constructor with the {typeof(DefConstructor)} attribute or method with the {typeof(DefFactory)} attribute.";
}
