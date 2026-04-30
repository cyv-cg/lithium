using System;

namespace Lithium.Core.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class EnforceInheritance : Attribute {
	public Type ParentType { get; }

	public EnforceInheritance(Type type) {
		ParentType = type;
	}
}
