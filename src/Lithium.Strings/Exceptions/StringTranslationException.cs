using System;
using System.Collections.Generic;
using System.Linq;
using Fluent.Net;

namespace Lithium.Strings.Exceptions;

/// <summary>
/// Exception thrown when there is an error during string translation.
/// </summary>
public class StringTranslationException : Exception {
	private readonly List<FluentError> errors;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => string.Join('\n', errors.Select(e => e.Message));

	/// <summary>
	/// Create an exception from a collection of FluentError objects.
	/// </summary>
	/// <param name="errors">Error collection from MessageContext.Format.</param>
	public StringTranslationException(ICollection<FluentError> errors) {
		this.errors = new List<FluentError>(errors);
	}
}
