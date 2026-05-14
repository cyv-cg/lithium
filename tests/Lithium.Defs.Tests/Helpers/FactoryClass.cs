using System.Xml;
using Lithium.Core.Attributes;

namespace Lithium.Defs.Tests;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[UseDefOverrideInitializer]
public class FactoryClass1 {
	public int tenPlus;

	[DefConstructor]
	public FactoryClass1(XmlNode node) {
		tenPlus = 10 + int.Parse(node.InnerText);
	}
}

[UseDefOverrideInitializer]
public class FactoryClass2 {
	public string? value;

	[DefFactory]
	public static FactoryClass2 Factory(XmlNode node) {
		FactoryClass2 item = new FactoryClass2 {
			value = node.InnerText
		};
		return item;
	}
}

[UseDefOverrideInitializer]
public class FactoryClass3 {
	public FactoryClass3() { }
}

[UseDefOverrideInitializer]
public class FactoryClass4 {
	[DefConstructor]
	public FactoryClass4() { }
}

[UseDefOverrideInitializer]
public class FactoryClass5 {
	[DefFactory]
	public static FactoryClass4 Factory() {
		return new FactoryClass4();
	}
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
