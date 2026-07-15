using System.Xml;
using Lithium.Core;

namespace Lithium.Defs.Tests;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class FactoryClass1 {
	public int tenPlus;

	[XmlFactory]
	public FactoryClass1(XmlNode node) {
		tenPlus = 10 + int.Parse(node.InnerText);
	}
}

public class FactoryClass2 {
	public string? value;

	[XmlFactory]
	public static FactoryClass2 Factory(XmlNode node) {
		FactoryClass2 item = new FactoryClass2 {
			value = node.InnerText
		};
		return item;
	}
}

public class FactoryClass3 {
	public FactoryClass3() { }
}

public class FactoryClass4 {
	[XmlFactory]
	public FactoryClass4() { }
}

public class FactoryClass5 {
	[XmlFactory]
	public static FactoryClass4 Factory() {
		return new FactoryClass4();
	}
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
