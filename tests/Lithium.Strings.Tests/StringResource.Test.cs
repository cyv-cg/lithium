using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fluent.Net;
using Lithium.Strings.Exceptions;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.StringResource.cs
/// </summary>
public class StringResourceTests {
	private readonly CultureInfo mockCulture = new CultureInfo("en-US");
	private readonly string mocksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__mocks__");

	#region Constructor
	/// <summary>
	/// Tests that the constructor takes in external resources properly.
	/// </summary>
	[Fact]
	public void ConstructorTest01() {
		StringResource resource = new StringResource(mockCulture, Path.Combine("path", "en-US", "to ", "resource.ftl"));

		Assert.Equal(Path.Combine("path", "en-US", "to ", "resource.ftl"), resource.ResourcePath);
		Assert.Equal("en-US", resource.Locale.Name);
		Assert.False(resource.Embedded);
	}
	/// <summary>
	/// Tests that the constructor throws an exception when passed an empty parameter for the resource path.
	/// </summary>
	[Fact]
	public void ConstructorTest02() {
		Exception ex = Assert.Throws<ArgumentNullException>(
			() => _ = new StringResource(mockCulture, "")
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that the constructor takes in embedded resources properly.
	/// </summary>
	[Fact]
	public void ConstructorTest03() {
		StringResource resource = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, Path.Combine("path", "en-US", "to ", "resource.ftl"));

		Assert.Equal(Path.Combine("path", "en-US", "to ", "resource.ftl"), resource.ResourcePath);
		Assert.Equal("en-US", resource.Locale.Name);
		Assert.NotNull(resource.Assembly);
		Assert.Equal(typeof(StringResourceTests).Assembly, resource.Assembly);
		Assert.True(resource.Embedded);
	}
	/// <summary>
	/// Tests that the constructor throws an exception when passed an empty parameter for the resource path.
	/// </summary>
	[Fact]
	public void ConstructorTest04() {
		Exception ex = Assert.Throws<ArgumentNullException>(
			() => _ = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, "")
		);
		Assert.NotNull(ex);
	}
	#endregion

	#region GetNamespace
	/// <summary>
	/// Tests that namespaces with a whole path are parsed properly.
	/// </summary>
	[Fact]
	public void GetNamespaceTest01() {
		string mockResourcePath = Path.Combine("home", "user", "Documents", "strings", "en-US", "namespace", "test", "content.ftl");

		StringResource resource = new StringResource(mockCulture, mockResourcePath);

		Assert.Equal("strings.namespace.test.content", resource.Namespace);
	}
	/// <summary>
	/// Tests that external namespaces with no path are parsed properly.
	/// </summary>
	[Fact]
	public void GetNamespaceTest02() {
		string mockResourcePath = "content.ftl";

		Exception ex = Assert.Throws<ResourceFormatException>(
			() => _ = new StringResource(mockCulture, mockResourcePath)
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that embedded namespaces with no locale are parsed properly.
	/// </summary>
	[Fact]
	public void GetNamespaceTest03() {
		string mockResourcePath = "strings@content.ftl";

		Exception ex = Assert.Throws<ResourceFormatException>(
			() => _ = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, mockResourcePath)
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that embedded namespaces with multiple roots are parsed properly.
	/// </summary>
	[Fact]
	public void GetNamespaceTest04() {
		string mockResourcePath = "strings@en-US@content.ftl";

		Exception ex = Assert.Throws<ResourceFormatException>(
			() => _ = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, mockResourcePath)
		);
		Assert.NotNull(ex);
	}
	#endregion

	#region GetAddresses
	/// <summary>
	/// Tests that addresses for external resources are parsed properly.
	/// </summary>
	[Fact]
	public void GetAddressesTest01() {
		StringResource resource = new StringResource(mockCulture, Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"));

		IEnumerable<string> addresses = resource.GetAddresses();
		Assert.Collection(addresses,
			s => {
				Assert.Equal("strings01.mockStrings01.sample-string", s);
			},
			s => {
				Assert.Equal("strings01.mockStrings01.string-with-one-placeable", s);
			},
			s => {
				Assert.Equal("strings01.mockStrings01.string-with-two-placeables", s);
			},
			s => {
				Assert.Equal("strings01.mockStrings01.string-with-bad-selector", s);
			}
		);
	}
	/// <summary>
	/// Tests that addresses for embedded resources are parsed properly.
	/// </summary>
	[Fact]
	public void GetAddressesTest02() {
		StringResource resource = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, "strings@en-US/embedded-strings.ftl");

		IEnumerable<string> addresses = resource.GetAddresses();
#pragma warning disable xUnit2023 // Do not use collection methods for single-item collections
		Assert.Collection(addresses,
			s => {
				Assert.Equal("strings.embedded-strings.test-value", s);
			}
		);
#pragma warning restore xUnit2023 // Do not use collection methods for single-item collections
	}
	/// <summary>
	/// Tests that there are no addresses if the resource could not be loaded.
	/// </summary>
	[Fact]
	public void GetAddressesTest03() {
		StringResource resource = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, "resource@en-US/that/does/not/exist.ftl");

		IEnumerable<string> addresses = resource.GetAddresses();
		Assert.Empty(addresses);
	}
	#endregion

	#region ToMessageContext
	/// <summary>
	/// Tests that a MessageContext is created with values from external resources.
	/// </summary>
	[Fact]
	public void ToMessageContextTest01() {
		StringResource resource = new StringResource(mockCulture, Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"));
		MessageContext? context = resource.ToMessageContext();

		Assert.NotNull(context);
		Assert.Equal(mockCulture, context.Culture);
		Assert.True(context.HasMessage("sample-string"));
		Assert.True(context.HasMessage("string-with-one-placeable"));
		Assert.True(context.HasMessage("string-with-two-placeables"));
		Assert.True(context.HasMessage("string-with-bad-selector"));
	}
	/// <summary>
	/// Tests that a MessageContext is created with values from embedded resources.
	/// </summary>
	[Fact]
	public void ToMessageContextTest02() {
		StringResource resource = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, "strings@en-US/embedded-strings.ftl");
		MessageContext? context = resource.ToMessageContext();

		Assert.NotNull(context);
		Assert.Equal(mockCulture, context.Culture);
		Assert.True(context.HasMessage("test-value"));
	}
	/// <summary>
	/// Tests that null is returned when the embedded resource could not be loaded.
	/// </summary>
	[Fact]
	public void ToMessageContextTest03() {
		StringResource resource = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, "resource@en-US/that/does/not/exist.ftl");
		MessageContext? context = resource.ToMessageContext();

		Assert.Null(context);
	}
	/// <summary>
	/// Tests that a ParseException is thrown when trying to load resources with errors.
	/// </summary>
	[Fact]
	public void ToMessageContextTest04() {
		// Create a temporary file with syntax errors dynamically so the editor isn't permanently complaining about it.
		string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_ = Directory.CreateDirectory(tempDirectory);
		_ = Directory.CreateDirectory(Path.Combine(tempDirectory, "en-US"));

		string filePath = Path.Combine(tempDirectory, "en-US", "mockStrings.ftl");
		FileStream file = File.Create(filePath);
		file.Close();
		File.WriteAllText(filePath, "syntax errors!");

		try {
			StringResource resource = new StringResource(mockCulture, filePath);

			Exception ex = Assert.Throws<ParseException>(
				() => _ = resource.ToMessageContext()
			);
			Assert.NotNull(ex);
		}
		finally {
			Directory.Delete(tempDirectory, true);
		}
	}
	#endregion

	#region ToString
	/// <summary>
	/// Tests that the string value of the external resource is the external resource path.
	/// </summary>
	[Fact]
	public void ToStringTest01() {
		StringResource resource = new StringResource(mockCulture, Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"));
		string value = resource.ToString();

		Assert.Equal(Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"), value);
	}
	/// <summary>
	/// Tests that the string value of the embedded resource is the assembly name.
	/// </summary>
	[Fact]
	public void ToStringTest02() {
		StringResource resource = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, "strings@en-US/embedded-strings.ftl");
		string value = resource.ToString();

		Assert.Equal(typeof(StringResourceTests).Assembly.GetName().ToString(), value);
	}
	#endregion

	#region Equals
	/// <summary>
	/// Tests that two StringResources with different paths are not equal.
	/// </summary>
	[Fact]
	public void EqualsTest01() {
		StringResource resource1 = new StringResource(mockCulture, Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"));
		StringResource resource2 = new StringResource(mockCulture, typeof(StringResourceTests).Assembly, "strings@en-US/embedded-strings.ftl");

		Assert.False(resource1.Equals(resource2));
	}
	/// <summary>
	/// Tests that two StringResources with the same path are equal.
	/// </summary>
	[Fact]
	public void EqualsTest02() {
		StringResource resource1 = new StringResource(mockCulture, Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"));
		StringResource resource2 = new StringResource(mockCulture, Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"));

		Assert.True(resource1.Equals(resource2));
	}
	/// <summary>
	/// Tests that a StringResource is not equal to an object of a different type.
	/// </summary>
	[Fact]
	public void EqualsTest03() {
		StringResource resource1 = new StringResource(mockCulture, Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"));
		string resource2 = resource1.ResourcePath;

		Assert.False(resource1.Equals(resource2));
	}
	#endregion
}
