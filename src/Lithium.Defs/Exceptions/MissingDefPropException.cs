using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception thrown when a def is missing required properties. The message will include the names of the missing properties.
/// </summary>
public class MissingDefPropException(string defKey, params PropertyInfo[] missingProps) : Exception {
	private readonly string defKey = defKey;
	private readonly IEnumerable<PropertyInfo> missingFields = missingProps;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"Missing fields in def '{defKey}': {string.Join(", ", missingFields.Select(f => f.Name))}";
}
