using System;
using System.Collections.Generic;
using System.Linq;
using Fluent.Net;

namespace Lithium.Strings.Exceptions;

/// <summary>
/// Exception thrown when there is an error during string translation.
/// </summary>
public class StringTranslationException : Exception {
	private List<FluentError> errors;

	public override string Message => string.Join('\n', errors.Select(e => e.Message));

	public StringTranslationException(ICollection<FluentError> errors) {
		this.errors = new List<FluentError>(errors);
	}
}
