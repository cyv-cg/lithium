using Lithium.Defs.Tests;
using System.Xml;
using Lithium.Core;

public static class TestAssembly02Class {
	[XmlFactory]
	public static DataClassWithExternalFactory Factory(XmlNode node) {
		DataClassWithExternalFactory data = new DataClassWithExternalFactory {
			Content = $"from TestAssembly02: {node.InnerText}"
		};
		return data;
	}
	[XmlFactory]
	public static DataClassWithExternalFactory Factory2(XmlNode node) {
		DataClassWithExternalFactory data = new DataClassWithExternalFactory {
			Content = $"from another factory in TestAssembly02: {node.InnerText}"
		};
		return data;
	}
}
