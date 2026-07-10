using Lithium.Core.Attributes;
using Lithium.Defs.Tests;
using System.Xml;

[UseDefOverrideInitializer]
public static class TestAssembly02Class {
	[DefFactory]
	public static DataClassWithExternalFactory Factory(XmlNode node) {
		DataClassWithExternalFactory data = new DataClassWithExternalFactory {
			Content = $"from TestAssembly02: {node.InnerText}"
		};
		return data;
	}
	[DefFactory]
	public static DataClassWithExternalFactory Factory2(XmlNode node) {
		DataClassWithExternalFactory data = new DataClassWithExternalFactory {
			Content = $"from another factory in TestAssembly02: {node.InnerText}"
		};
		return data;
	}
}
