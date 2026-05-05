using System;
using Lithium.Strings;

namespace Lithium.Defs;

public record Def {
	public required string Key { get; init; }
	public required KeyedString Label { get; init; }
}
