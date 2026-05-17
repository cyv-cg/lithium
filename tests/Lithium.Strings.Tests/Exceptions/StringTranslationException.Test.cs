using System.Collections.Generic;
using Fluent.Net;
using Lithium.Strings.Exceptions;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.Exceptions.StringTranslationException.cs
/// </summary>
public class StringTranslationExceptionTests {
	private class MockFluentError : FluentError {
		public MockFluentError(string message) : base(message) { }
	}

	/// <summary>
	/// Tests that the constructor creates the expected message for multiple errors.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		List<FluentError> errors = new List<FluentError> {
			new MockFluentError("Error 1"),
			new MockFluentError("Error 2")
		};

		StringTranslationException ex = new StringTranslationException(errors);

		Assert.Equal("Error 1\nError 2", ex.Message);
	}
	/// <summary>
	/// Tests that the constructor creates the expected message for a single error.
	/// </summary>
	[Fact]
	public void ConstructorTest02() {
		List<FluentError> errors = new List<FluentError> {
			new MockFluentError("Error 1")
		};

		StringTranslationException ex = new StringTranslationException(errors);

		Assert.Equal("Error 1", ex.Message);
	}
}
