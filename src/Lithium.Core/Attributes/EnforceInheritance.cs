using System;

namespace Lithium.Core.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class EnforceInheritance<T> : Attribute {
	public Type ParentType { get; }

	public EnforceInheritance() {
		ParentType = typeof(T);
	}
}
