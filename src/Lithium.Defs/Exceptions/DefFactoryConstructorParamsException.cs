using System;
using System.Xml;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception indicating a def factory has an invalid parameter list.
/// </summary>
public class DefFactoryConstructorParamsException(Type defType) : Exception {
	private readonly Type defType = defType;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"Constructor/Factory for type {defType} must take {typeof(XmlNode)} as the sole parameter.";
}
