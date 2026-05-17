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

	/// <summary>
	/// Message describing the error.
	/// </summary>
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

	/// <summary>
	/// Indicates that a Def class needs to inherit from a given type.
	/// </summary>
	/// <param name="defKey">Key for the problem def.</param>
	/// <param name="type">The def class's type.</param>
	/// <param name="targetType">The type it should be inheriting from.</param>
	public DefInheritanceException(string defKey, Type type, Type targetType) {
		this.defKey = defKey;
		this.type = type;
		this.targetType = targetType;
	}
	/// <summary>
	/// Indicates that a specific property on a Def needs to inherit from a given type.
	/// </summary>
	/// <param name="defKey">Key for the problem def.</param>
	/// <param name="propName">Name of the problem property.</param>
	/// <param name="type">The property's type.</param>
	/// <param name="targetType">The type the property needs to inherit.</param>
	/// <returns></returns>
	public DefInheritanceException(string defKey, string propName, Type type, Type targetType) : this(defKey, type, targetType) {
		this.propName = propName;
	}
}
