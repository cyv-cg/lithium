using System;
using Lithium.Strings;

namespace Lithium.Defs;

public class Def {
	public required string key;
	public required KeyedString label;

	public override string ToString() {
		return label.ToString();
	}
}
