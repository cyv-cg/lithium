using System.Collections.Generic;
using Fluent.Net;
using Lithium.Strings.Exceptions;
using Xunit;

namespace Lithium.Strings.Tests;

public class StringTranslationExceptionTests {
	private class MockFluentError : FluentError {
		public MockFluentError(string message) : base(message) { }
	}

	[Fact]
	public void ConstructorTest01() {
		List<FluentError> errors = new List<FluentError> {
			new MockFluentError("Error 1"),
			new MockFluentError("Error 2")
		};

		StringTranslationException ex = new StringTranslationException(errors);

		Assert.Equal("Error 1\nError 2", ex.Message);
	}

	[Fact]
	public void ConstructorTest02() {
		List<FluentError> errors = new List<FluentError> {
			new MockFluentError("Error 1")
		};

		StringTranslationException ex = new StringTranslationException(errors);

		Assert.Equal("Error 1", ex.Message);
	}
}
