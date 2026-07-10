using System.Xml;
using Lithium.Core.Attributes;

namespace Lithium.Defs.Tests;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[UseDefOverrideInitializer]
public static class StaticFactoryMethods {
	[DefFactory]
	public static DataClassWithExternalFactory Factory(XmlNode node) {
		DataClassWithExternalFactory data = new DataClassWithExternalFactory {
			Content = $"Content: {node.InnerText}"
		};
		return data;
	}
}

[UseDefOverrideInitializer]
public static class MoreStaticFactoryMethods {
	[DefFactory]
	public static DataClassWithExternalFactory Factory(XmlNode node) {
		DataClassWithExternalFactory data = new DataClassWithExternalFactory {
			Content = $"Different content: {node.InnerText}"
		};
		return data;
	}
}

public class DataClassWithExternalFactory {
	public required string Content { get; set; }
}

public class ExtFactoryDef : Def {
	public required DataClassWithExternalFactory Data { get; init; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
