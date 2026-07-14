using System;
using System.Text;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception thrown when a parsed Def fails its validation check.
/// </summary>
public class DefValidationException(Def def, StringBuilder? errors = null) : Exception {
	/// <inheritdoc/>
	public override string Message {
		get {
			if (errors != null) {
				return $"The following error(s) occurred while validating def '{def.Key}': {errors}";
			}
			return $"An error occurred validating def '{def.Key}'.";
		}
	}
}
