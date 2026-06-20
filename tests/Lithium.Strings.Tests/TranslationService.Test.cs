using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Fluent.Net;
using Fluent.Net.RuntimeAst;
using Lithium.Strings.Exceptions;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.TranslationService.cs
/// </summary>
public class TranslationServiceTests {
	private class TestTranslationService : TranslationService {
		public HashSet<StringResource> Resources => resources;
		public Dictionary<string, MessageContext> Contexts => contexts;

		public TestTranslationService(TranslationServiceOptions options) : base(options) { }

		public new bool TryGetMessage(string address, out MessageContext? context, out Message? message) {
			return base.TryGetMessage(address, out context, out message);
		}
#pragma warning disable CA1822 // Mark members as static
		public new Dictionary<string, object> FormatArgs(params (string key, object value)[] args) {
			return TranslationService.FormatArgs(args);
		}
		public new (string @namespace, string key) ParseAddress(string address) {
			return TranslationService.ParseAddress(address);
		}
#pragma warning restore CA1822 // Mark members as static
	}

	private static readonly string mocksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__mocks__");

	private TestTranslationService service;
	/// <summary>
	/// Initialize a default service between runs.
	/// </summary>
	public TranslationServiceTests() {
		TranslationServiceOptions options = new TranslationServiceOptions {
			PrimaryLocale = new CultureInfo("en-US")
		};
		service = new TestTranslationService(options);
	}

	#region RegisterResource
	/// <summary>
	/// Tests that external resources get registered properly.
	/// </summary>
	[Fact]
	public void RegisterResourceTest01() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));

		Assert.True(success);
		Assert.Collection(service.Resources,
			r => {
				Assert.Equal(Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"), r.ResourcePath);
			},
			r => {
				Assert.Equal(Path.Combine(mocksDirectory, "strings01", "en-US", "sub", "mockStrings02.ftl"), r.ResourcePath);
			},
			r => {
				Assert.Equal(Path.Combine(mocksDirectory, "strings01", "en-US", "sub", "mockStrings03.ftl"), r.ResourcePath);
			}
		);
	}
	/// <summary>
	/// Tests than a <see cref="DirectoryNotFoundException"/> is thrown when trying to register a directory that does not exist.
	/// </summary>
	[Fact]
	public void RegisterResourceTest02() {
		Exception ex = Assert.Throws<DirectoryNotFoundException>(
			() => service.RegisterResource(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()))
		);
		Assert.NotNull(ex);
		Assert.Empty(service.Resources);
	}
	/// <summary>
	/// Tests that an <see cref="ArgumentNullException"/> is thrown when given an empty string input.
	/// </summary>
	[Fact]
	public void RegisterResourceTest03() {
		Exception ex = Assert.Throws<ArgumentNullException>(
			() => service.RegisterResource("")
		);
		Assert.NotNull(ex);
		Assert.Empty(service.Resources);
	}
	/// <summary>
	/// Tests that the external resource fails to register if it does not have a locale sub-directory.
	/// </summary>
	[Fact]
	public void RegisterResourceTest04() {
		string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_ = Directory.CreateDirectory(tempDirectory);

		try {
			bool success = service.RegisterResource(tempDirectory);
			Assert.False(success);
			Assert.Empty(service.Resources);
		}
		finally {
			Directory.Delete(tempDirectory);
		}
	}
	/// <summary>
	/// Tests that an external resource only gets added once when registered multiple times.
	/// </summary>
	[Fact]
	public void RegisterResourceTest05() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));

		Assert.False(success);
		Assert.Collection(service.Resources,
			r => {
				Assert.Equal(Path.Combine(mocksDirectory, "strings01", "en-US", "mockStrings01.ftl"), r.ResourcePath);
			},
			r => {
				Assert.Equal(Path.Combine(mocksDirectory, "strings01", "en-US", "sub", "mockStrings02.ftl"), r.ResourcePath);
			},
			r => {
				Assert.Equal(Path.Combine(mocksDirectory, "strings01", "en-US", "sub", "mockStrings03.ftl"), r.ResourcePath);
			}
		);
	}

	/// <summary>
	/// Tests that embedded resources get registered properly.
	/// </summary>
	[Fact]
	public void RegisterResourceTest06() {
		bool success = service.RegisterResource(typeof(TranslationServiceTests).Assembly);

		Assert.True(success);
		Assert.Collection(service.Resources,
			r => {
				Assert.Equal("strings@" + Path.Combine("en-US", "embedded-strings.ftl"), r.ResourcePath);
			},
			r => {
				Assert.Equal("strings01@" + Path.Combine("en-US", "mockStrings01.ftl"), r.ResourcePath);
			}
		);
	}
	/// <summary>
	/// Tests that the resource is not registered if it does not have any strings.
	/// </summary>
	[Fact]
	public void RegisterResourceTest07() {
		bool success = service.RegisterResource(typeof(ITranslationService).Assembly);
		Assert.False(success);
		Assert.Empty(service.Resources);
	}
	/// <summary>
	/// Tests that an embedded resource only gets added once when registered multiple times.
	/// </summary>
	[Fact]
	public void RegisterResourceTest08() {
		_ = service.RegisterResource(typeof(TranslationServiceTests).Assembly);
		bool success = service.RegisterResource(typeof(TranslationServiceTests).Assembly);

		Assert.False(success);
		Assert.Collection(service.Resources,
			r => {
				Assert.Equal("strings@" + Path.Combine("en-US", "embedded-strings.ftl"), r.ResourcePath);
			},
			r => {
				Assert.Equal("strings01@" + Path.Combine("en-US", "mockStrings01.ftl"), r.ResourcePath);
			}
		);
	}
	#endregion

	#region Reload
	private class ReloadTestService : TranslationService {
		public ReloadTestService(TranslationServiceOptions options) : base(options) { }
#pragma warning disable IDE0060 // Remove unused parameter
		public new bool RegisterResource(Assembly? assembly) {
			_ = resources.Add(new StringResource(new CultureInfo("en-US"), typeof(TranslationServiceTests).Assembly, "resource/en-US/that/does/not/exist.ftl"));
			return true;
		}
#pragma warning restore IDE0060 // Remove unused parameter
	}

	/// <summary>
	/// Tests that strings get loaded from external resources.
	/// </summary>
	[Fact]
	public void ReloadTest01() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.Collection(service.Contexts.Keys,
			k => {
				Assert.Equal("strings01.mockStrings01", k);
			},
			k => {
				Assert.Equal("strings01.sub.mockStrings02", k);
			},
			k => {
				Assert.Equal("strings01.sub.mockStrings03", k);
			}
		);
	}
	/// <summary>
	/// Tests that strings get loaded from embedded resources.
	/// </summary>
	[Fact]
	public void ReloadTest02() {
		_ = service.RegisterResource(typeof(TranslationServiceTests).Assembly);
		service.Reload();

		Assert.Contains("strings.embedded-strings", service.Contexts.Keys);
	}
	/// <summary>
	/// Tests that there are no strings loaded if no resources have been registed.
	/// </summary>
	[Fact]
	public void ReloadTest03() {
		service.Reload();
		Assert.Empty(service.Resources);
	}
	/// <summary>
	/// Tests that an exception is thrown when trying to load multiple resources with the same namespace.
	/// </summary>
	[Fact]
	public void ReloadTest04() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		_ = service.RegisterResource(typeof(TranslationServiceTests).Assembly);

		Exception ex = Assert.Throws<InvalidOperationException>(
			service.Reload
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that no contexts for embedded resources that fail to load.
	/// </summary>
	[Fact]
	public void ReloadTest05() {
		ReloadTestService testService = new ReloadTestService(new TranslationServiceOptions { PrimaryLocale = new CultureInfo("en-US") });
		_ = testService.RegisterResource(null);
		testService.Reload();
		Assert.Empty(service.Contexts);
	}
	#endregion

	#region GetAllStringAddresses
	/// <summary>
	/// Tests that addresses are found from external resources.
	/// </summary>
	[Fact]
	public void GetAllStringAddressesTest01() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		IEnumerable<string> addresses = service.GetAllStringKeys();

		Assert.Collection(addresses,
			key => {
				Assert.Equal("strings01.mockStrings01.sample-string", key);
			},
			key => {
				Assert.Equal("strings01.mockStrings01.string-with-one-placeable", key);
			},
			key => {
				Assert.Equal("strings01.mockStrings01.string-with-two-placeables", key);
			},
			key => {
				Assert.Equal("strings01.mockStrings01.string-with-bad-selector", key);
			},
			key => {
				Assert.Equal("strings01.sub.mockStrings02.sample-string", key);
			},
			key => {
				Assert.Equal("strings01.sub.mockStrings03.another-sample-string", key);
			}
		);
	}
	/// <summary>
	/// Tests that addresses are found from embedded resources.
	/// </summary>
	[Fact]
	public void GetAllStringAddressesTest02() {
		_ = service.RegisterResource(typeof(TranslationServiceTests).Assembly);
		service.Reload();

		IEnumerable<string> addresses = service.GetAllStringKeys();

#pragma warning disable xUnit2023 // Do not use collection methods for single-item collections
		Assert.Collection(addresses,
			key => {
				Assert.Equal("strings.embedded-strings.test-value", key);
			}
		);
#pragma warning restore xUnit2023 // Do not use collection methods for single-item collections
	}
	/// <summary>
	/// Tests that no addresses are found when no resources have been registered.
	/// </summary>
	[Fact]
	public void GetAllStringAddressesTest03() {
		IEnumerable<string> addresses = service.GetAllStringKeys();
		Assert.Empty(addresses);
	}
	#endregion

	#region Translate
	/// <summary>
	/// Tests that loaded strings translate properly.
	/// </summary>
	[Fact]
	public void TranslateTest01() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Assert.Equal("test", service.Translate("strings01.mockStrings01.sample-string"));
	}
	/// <summary>
	/// Tests that strings in a sub-namespace translate properly.
	/// </summary>
	[Fact]
	public void TranslateTest02() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Assert.Equal("namespace test", service.Translate("strings01.sub.mockStrings02.sample-string"));
	}
	/// <summary>
	/// Tests that parameters are inserted into translated strings.
	/// </summary>
	[Fact]
	public void TranslateTest03() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Assert.Equal("value: 5", service.Translate("strings01.mockStrings01.string-with-one-placeable", ("data", 5)));
	}
	/// <summary>
	/// Tests that multiple parameters are inserted into translated strings.
	/// </summary>
	[Fact]
	public void TranslateTest04() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Assert.Equal("value1: 5, value2: 6", service.Translate("strings01.mockStrings01.string-with-two-placeables", ("data1", 5), ("data2", 6)));
	}
	/// <summary>
	/// Tests that a KeyNotFoundException is thrown when trying to translate a string that does not exist.
	/// </summary>
	[Fact]
	public void TranslateTest06() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Exception? ex = Assert.Throws<KeyNotFoundException>(
			() => service.Translate("key-that-does-not-exist")
		);
	}
	/// <summary>
	/// Tests that exceptions are thrown when there is an error in loading the translation.
	/// </summary>
	[Fact]
	public void TranslateTest07() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Exception? ex = Assert.Throws<StringTranslationException>(
			() => service.Translate("strings01.mockStrings01.string-with-bad-selector")
		);
	}
	/// <summary>
	/// Tests that an ArgumentException is thrown when a string parameter key is empty.
	/// </summary>
	[Fact]
	public void TranslateTest08() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Exception? ex = Assert.Throws<ArgumentException>(
			() => service.Translate("strings01.mockStrings01.string-with-one-placeable", ("", 5))
		);
	}
	/// <summary>
	/// Tests that a KeyNotFoundException is thrown when the namespace exists but the key does not.
	/// </summary>
	[Fact]
	public void TranslateTest09() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Exception? ex = Assert.Throws<KeyNotFoundException>(
			() => service.Translate("strings01.sub.mockStrings02.key-that-does-not-exist")
		);
	}
	/// <summary>
	/// Tests that a KeyNotFoundException is thrown when neither the namespace nor the key exists.
	/// </summary>
	[Fact]
	public void TranslateTest10() {
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.True(success);
		Exception? ex = Assert.Throws<KeyNotFoundException>(
			() => service.Translate("namespace.that.does.not.exist.key-that-does-not-exist")
		);
	}
	/// <summary>
	/// Test that unicode characters in translations are handled properly.
	/// </summary>
	[Fact]
	public void TranslateTest11() {
		service = new TestTranslationService(new TranslationServiceOptions { PrimaryLocale = new CultureInfo("ja-JP") });
		bool success = service.RegisterResource(Path.Combine(mocksDirectory, "strings02"));
		service.Reload();

		Assert.True(success);
		Assert.Equal("サンプル", service.Translate("strings02.mockStrings.test-string"));
	}
	#endregion

	#region HasMessage
	/// <summary>
	/// Tests that when an address exists in the context, HasMessage returns true.
	/// </summary>
	[Fact]
	public void HasMessageTest01() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();
		Assert.True(service.HasMessage("strings01.mockStrings01.sample-string"));
	}
	/// <summary>
	/// Tests that when an address does not exist in the context, HasMessage returns false.
	/// </summary>
	[Fact]
	public void HasMessageTest02() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();
		Assert.False(service.HasMessage("address.that.does.not.exist"));
	}
	#endregion

	#region FormatArgs
	/// <summary>
	/// Tests that an array or key-value tuples gets properly mapped into a dictionary.
	/// </summary>
	[Fact]
	public void FormatArgsTest01() {
		Dictionary<string, object> map = service.FormatArgs(("key", "value"), ("another-key", 2));
		Assert.Collection(map,
			v => {
				Assert.Equal("key", v.Key);
				Assert.Equal("value", v.Value);
			},
			v => {
				Assert.Equal("another-key", v.Key);
				Assert.Equal(2, v.Value);
			}
		);
	}
	/// <summary>
	/// Tests that an <see cref="ArgumentException"/> is thrown when a key is null.
	/// </summary>
	[Fact]
	public void FormatArgsTest02() {
		Exception ex = Assert.Throws<ArgumentException>(
			() => _ = service.FormatArgs(("", "value"))
		);
		Assert.NotNull(ex);
	}
	#endregion

	#region TryGetMessage
	/// <summary>
	/// Tests that the context and message are returned when the string exists.
	/// </summary>
	[Fact]
	public void TryGetMessageTest01() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		bool success = service.TryGetMessage("strings01.mockStrings01.sample-string", out MessageContext? context, out Message? message);

		Assert.True(success);
		Assert.NotNull(context);
		Assert.NotNull(message);
	}
	/// <summary>
	/// Tests that both the context and message are null when the address does not exist.
	/// </summary>
	[Fact]
	public void TryGetMessageTest02() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		bool success = service.TryGetMessage("key-that-does-not-exist", out MessageContext? context, out Message? message);

		Assert.False(success);
		Assert.Null(context);
		Assert.Null(message);
	}
	/// <summary>
	/// Tests that the context is returned but not the message when a valid namespace and non-existant key are given.
	/// </summary>
	[Fact]
	public void TryGetMessageTest03() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		bool success = service.TryGetMessage("strings01.mockStrings01.key-that-does-not-exist", out MessageContext? context, out Message? message);

		Assert.False(success);
		Assert.NotNull(context);
		Assert.Null(message);
	}
	#endregion

	#region ParseAddress
	/// <summary>
	/// Tests that ParseAddress can correctly separate the namespace and key.
	/// </summary>
	[Fact]
	public void ParseAddressTest01() {
		string mockAddress = "strings.namespace.address.key";

		Assert.Equal(("strings.namespace.address", "key"), service.ParseAddress(mockAddress));
	}
	/// <summary>
	/// Tests that ParseAddress throws an <see cref="ArgumentNullException"/> when given an empty input.
	/// </summary>
	[Fact]
	public void ParseAddressTest02() {
		string mockAddress = "";

		Exception ex = Assert.Throws<ArgumentNullException>(
			() => service.ParseAddress(mockAddress)
		);
		Assert.NotNull(ex);
	}
	/// <summary>
	/// Tests that ParseAddress returns an empty namespace when given only a key.
	/// </summary>
	[Fact]
	public void ParseAddressTest03() {
		string mockAddress = "key";

		Assert.Equal(("", "key"), service.ParseAddress(mockAddress));
	}
	#endregion

	#region CompareCompletion
	private const float EPSILON = 1e-12f;

	private class OtherTranslationService1 : ITranslationService {
		public IEnumerable<string> GetAllStringKeys() {
			return new string[] {
				"strings01.mockStrings01.sample-string",
				"strings01.mockStrings01.string-with-one-placeable",
				"strings01.sub.mockStrings02.sample-string",
				"strings01.sub.mockStrings02.string-that-does-not-exist-in-base-service"
			};
		}

		public bool HasMessage(string key) {
			return GetAllStringKeys().Contains(key);
		}

		public void Reload() {
			throw new NotImplementedException();
		}

		public string Translate(string key, params (string key, object value)[] args) {
			throw new NotImplementedException();
		}
	}
	private class OtherTranslationService2 : ITranslationService {
		public IEnumerable<string> GetAllStringKeys() {
			return new string[] {
				"strings01.mockStrings01.sample-string",
				"strings01.mockStrings01.string-with-one-placeable",
				"strings01.mockStrings01.string-with-two-placeables",
				"strings01.mockStrings01.string-with-bad-selector",
				"strings01.sub.mockStrings02.sample-string",
				"strings01.sub.mockStrings03.another-sample-string",
				"strings01.sub.mockStrings02.string-that-does-not-exist-in-base-service"
			};
		}

		public bool HasMessage(string key) {
			return GetAllStringKeys().Contains(key);
		}

		public void Reload() {
			throw new NotImplementedException();
		}

		public string Translate(string key, params (string key, object value)[] args) {
			throw new NotImplementedException();
		}
	}
	private class OtherTranslationService3 : ITranslationService {
		public IEnumerable<string> GetAllStringKeys() {
			return Array.Empty<string>();
		}

		public bool HasMessage(string key) {
			return GetAllStringKeys().Contains(key);
		}

		public void Reload() {
			throw new NotImplementedException();
		}

		public string Translate(string key, params (string key, object value)[] args) {
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Tests that CompareCompletion returns 100% when comparing a service to itself.
	/// </summary>
	[Fact]
	public void CompareCompletionTest01() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		float completion = service.CompareCompletion(service);
		Assert.InRange(completion, 1f - EPSILON, 1f + EPSILON);
	}
	/// <summary>
	/// Tests that CompareCompletion accurately counts addresses found in the reference service, but ignores ones that are not.
	/// </summary>
	[Fact]
	public void CompareCompletionTest02() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		ITranslationService instance = new OtherTranslationService1();
		float completion = service.CompareCompletion(instance);

		Assert.InRange(completion, (3f / 6) - EPSILON, (3f / 6) + EPSILON);
	}
	/// <summary>
	/// Tests that CompareCompletion returns 100% when the other service contains all addresses present in the reference.
	/// </summary>
	[Fact]
	public void CompareCompletionTest03() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		ITranslationService instance = new OtherTranslationService2();
		float completion = service.CompareCompletion(instance);

		Assert.InRange(completion, 1f - EPSILON, 1f + EPSILON);
	}
	/// <summary>
	/// Tests that when the reference service is either not initialized or has no strings, CompareCompletion returns -1.
	/// </summary>
	[Fact]
	public void CompareCompletionTest04() {
		ITranslationService instance = new OtherTranslationService1();
		float completion = service.CompareCompletion(instance);

		Assert.InRange(completion, -1f - EPSILON, -1f + EPSILON);
	}
	/// <summary>
	/// Tests then when the other service has no strings, CompareCompletion returns 0%.
	/// </summary>
	[Fact]
	public void CompareCompletionTest05() {
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		ITranslationService instance = new OtherTranslationService3();
		float completion = service.CompareCompletion(instance);

		Assert.InRange(completion, -EPSILON, EPSILON);
	}
	#endregion
}
