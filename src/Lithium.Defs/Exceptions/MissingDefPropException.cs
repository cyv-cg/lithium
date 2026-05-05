using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lithium.Defs.Exceptions;

public class MissingDefPropException(string defKey, params PropertyInfo[] missingProps) : Exception {
	private readonly string defKey = defKey;
	private readonly IEnumerable<PropertyInfo> missingFields = missingProps;

	public override string Message => $"Missing fields in def '{defKey}': {string.Join(", ", missingFields.Select(f => f.Name))}";
}
