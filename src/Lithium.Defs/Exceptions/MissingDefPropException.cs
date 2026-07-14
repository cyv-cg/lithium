using System;
using System.Linq;
using System.Reflection;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception thrown when a def is missing required properties. The message will include the names of the missing properties.
/// </summary>
public class MissingDefPropException(string defKey, string? subPropertyName = null, params PropertyInfo[] missingProps) : Exception {
	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message {
		get {
			if (string.IsNullOrEmpty(subPropertyName)) {
				return $"Missing fields in def '{defKey}': {string.Join(", ", missingProps.Select(f => f.Name))}";
			}
			else {
				return $"Missing fields in def '{defKey}' property '{subPropertyName}': {string.Join(", ", missingProps.Select(f => f.Name))}";
			}
		}
	}
}
