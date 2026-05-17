using System;

namespace Lithium.Core.Attributes;

/// <summary>
/// Attribute marking a function as the primary way to initialize a class as a def property.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DefFactory : Attribute { }
