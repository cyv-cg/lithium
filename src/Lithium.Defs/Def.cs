using Lithium.Strings;

namespace Lithium.Defs;

/// <summary>
/// Root structure for all Defs.
/// </summary>
public record Def {
	/// <summary>
	/// Primary key used to solely define the object.
	/// Must be distinct from all other Defs.
	/// </summary>
	public required string Key { get; init; }
	/// <summary>
	/// String name for the Def.
	/// </summary>
	public required KeyedString Label { get; init; }
}
