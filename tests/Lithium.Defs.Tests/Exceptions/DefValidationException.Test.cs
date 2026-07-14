using System.Text;
using Lithium.Defs.Exceptions;
using Xunit;

namespace Lithium.Defs.Tests;


/// <summary>
/// Tests for Lithium.Defs.Exceptions.DefValidationException
/// </summary>
public class DefValidationExceptionTests {
	private readonly DefService service;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public DefValidationExceptionTests() {
		service = new DefService(new DefServiceOptions());
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

	/// <summary>
	/// Tests that the message is as expected when not given any error details.
	/// </summary>
	[Fact]
	public void MessageTest01() {
		Init.Setup(1, service);
		DefValidationException ex = new DefValidationException(service.LoadDef<MockDef1>("MockDef")!);

		Assert.Equal(
			"An error occurred validating def 'MockDef'.",
			ex.Message
		);
	}
	/// <summary>
	/// Tests that the message is as expected when given error details.
	/// </summary>
	[Fact]
	public void MessageTest02() {
		Init.Setup(1, service);
		StringBuilder builder = new StringBuilder("error content");

		DefValidationException ex = new DefValidationException(service.LoadDef<MockDef1>("MockDef")!, builder);

		Assert.Equal(
			"The following error(s) occurred while validating def 'MockDef': error content",
			ex.Message
		);
	}
}
