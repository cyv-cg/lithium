using System;

namespace Lithium.Defs;

/// <summary>
/// Attribute indicating that a type property on a def needs to inherit from a specific type.
/// </summary>
/// <typeparam name="T">Type that needs to be inherited from.</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public class EnforceInheritance<T> : Attribute {
	/// <summary>
	/// Type that needs to be inherited from.
	/// </summary>
	public Type ParentType { get; }

	/// <summary>
	/// Instantiate the attribute with the given restriction.
	/// </summary>
	public EnforceInheritance() {
		ParentType = typeof(T);
	}
}
