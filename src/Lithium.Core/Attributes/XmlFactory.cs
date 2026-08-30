using System;

namespace Lithium.Core;

/// <summary>
/// Attribute marking a function as the primary way to initialize a class from XML.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public class XmlFactory : Attribute { }
