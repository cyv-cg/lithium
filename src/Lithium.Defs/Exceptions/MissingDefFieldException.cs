using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lithium.Defs.Exceptions;

public class MissingDefFieldException(string defKey, params FieldInfo[] missingFields) : Exception {
	private readonly string defKey = defKey;
	private readonly IEnumerable<FieldInfo> missingFields = missingFields;

	public override string Message => $"Missing fields in def '{defKey}': {string.Join(", ", missingFields.Select(f => f.Name))}";
}
