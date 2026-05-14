using System;

namespace Lithium.Core.Attributes;

/// <summary>
/// Attribute indicating that a type has a special constructor to use when loading it as a def.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class UseDefOverrideInitializer : Attribute { }
