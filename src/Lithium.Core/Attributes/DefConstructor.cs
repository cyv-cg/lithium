using System;

namespace Lithium.Core.Attributes;

/// <summary>
/// Attribute marking a constructor as the primary way to initialize a class as a def property.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class DefConstructor : Attribute { }
