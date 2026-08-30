using System.Xml;
using Lithium.Core;

namespace Lithium.Defs.Tests;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public static class StaticFactoryMethods {
	[XmlFactory]
	public static DataClassWithExternalFactory Factory(XmlNode node) {
		DataClassWithExternalFactory data = new DataClassWithExternalFactory {
			Content = $"Content: {node.InnerText}"
		};
		return data;
	}
}

public static class MoreStaticFactoryMethods {
	[XmlFactory]
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
