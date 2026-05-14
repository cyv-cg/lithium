using System;
using System.Xml;

namespace Lithium.Defs.Exceptions;

public class DefFactoryConstructorParamsException(Type defType) : Exception {
	private readonly Type defType = defType;

	public override string Message => $"Constructor/Factory for type {defType} must take {typeof(XmlNode)} as the sole parameter.";
}
