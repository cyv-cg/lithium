using System;

namespace Lithium.Defs.Exceptions;

public class PropertyLoadException(string defKey, string propName, string propValueRaw, Type propType) : Exception {
	private readonly string defKey = defKey;
	private readonly string propName = propName;
	private readonly string propValueRaw = propValueRaw;
	private readonly Type propType = propType;

	public override string Message => $"Def '{defKey}': could not apply value '{propValueRaw}' to property '{propName}' ({propType}).";
}
