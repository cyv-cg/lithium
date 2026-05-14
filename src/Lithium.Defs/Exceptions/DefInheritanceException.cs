using System;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception thrown when a type does not meet its inheritence restriction.
/// </summary>
public class DefInheritanceException : Exception {
	private readonly string defKey;
	private readonly Type type;
	private readonly Type targetType;
	private readonly string? propName;

	public override string Message {
		get {
			if (string.IsNullOrEmpty(propName)) {
				return $"Def '{defKey}': {type} must inherit {targetType}.";
			}
			else {
				return $"Def '{defKey}': property '{propName}' is type {type} which does not inherit {targetType}.";
			}
		}
	}

	public DefInheritanceException(string defKey, Type type, Type targetType) {
		this.defKey = defKey;
		this.type = type;
		this.targetType = targetType;
	}
	public DefInheritanceException(string defKey, string propName, Type type, Type targetType) : this(defKey, type, targetType) {
		this.propName = propName;
	}
}
