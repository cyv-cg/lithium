using System;
using System.Collections.Generic;

namespace Lithium.Defs;

/// <summary>
/// Settings for a <see cref="DefService"/>
/// </summary>
public class DefServiceOptions {
	/// <summary>
	/// Deferred Parsing will wait for a def to be used before parsing it from XML.
	/// Non-Deferred Parsing will immediately parse all defs at startup.
	/// </summary>
	public bool DeferredLoad { get; set; } = true;

	/// <summary>
	/// Custom default ID generator for Defs. This is used to generate a unique ID for each Def based on its key.
	/// The input is the UTF-8 bytes of the Def's key, and the output is a generated <see cref="uint"/> ID.
	/// </summary>
	public Func<byte[], uint>? DefaultIDGenerator;
	/// <summary>
	/// Custom ID generators for specific Def types. This allows for different ID generation strategies based on the type of Def being processed.
	/// The input is the UTF-8 bytes of the Def's key, and the output is a generated <see cref="uint"/> ID.
	/// </summary>
	public Dictionary<Type, Func<byte[], uint>> IDGenerators = new();
}
