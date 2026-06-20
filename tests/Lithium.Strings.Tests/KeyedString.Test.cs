using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Lithium.Core;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.KeyedString.cs
/// </summary>
public class KeyedStringTests {
	private static readonly string mocksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__mocks__");

	private TranslationService service;
	/// <summary>
	/// Initialize a default service between runs.
	/// </summary>
	public KeyedStringTests() {
		TranslationServiceOptions options = new TranslationServiceOptions {
			PrimaryLocale = new CultureInfo("en-US")
		};
		service = new TranslationService(options);
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();
	}

	#region Factory tests
	/// <summary>
	/// Tests that a KeyedString can be created from an XML node with no namespace.
	/// </summary>
	[Fact]
	public void FactoryTest01() {
		XmlNode node = XmlLoader.LoadDocument(Path.Combine(mocksDirectory, "defs", "mockDefs.xml"))!.FirstChild!.ChildNodes[0]!;

		KeyedString keyedString = KeyedString.Factory(node.SelectSingleNode("Label")!);

		Assert.Equal("MockDef_Label", keyedString.Key);
		Assert.Equal("", keyedString.Namespace);
		Assert.Equal("MockDef_Label", keyedString.Address);
	}
	/// <summary>
	/// Tests that a KeyedString can be created from an XML node with a namespace.
	/// </summary>
	[Fact]
	public void FactoryTest02() {
		XmlNode node = XmlLoader.LoadDocument(Path.Combine(mocksDirectory, "defs", "mockDefs.xml"))!.FirstChild!.ChildNodes[1]!;

		KeyedString keyedString = KeyedString.Factory(node.SelectSingleNode("Label")!);

		Assert.Equal("MockDef_Label", keyedString.Key);
		Assert.Equal("sub.namespace", keyedString.Namespace);
		Assert.Equal("sub.namespace.MockDef_Label", keyedString.Address);
	}
	#endregion

	#region Translate tests
	/// <summary>
	/// Tests that a KeyedString can be translated properly.
	/// </summary>
	[Fact]
	public void TranslateTest01() {
		Assert.Equal("test", ((KeyedString)"strings01.mockStrings01.sample-string").Translate(service));
	}
	/// <summary>
	/// Tests that an unloaded KeyedString returns its address when translated.
	/// </summary>
	[Fact]
	public void TranslateTest02() {
		Assert.Equal("key-that-does-not-exist", ((KeyedString)"key-that-does-not-exist").Translate(service));
	}
	/// <summary>
	/// Tests that a KeyedString can be translated properly with parameters replaced.
	/// </summary>
	[Fact]
	public void TranslateTest03() {
		KeyedString keyedString = (KeyedString)"strings01.mockStrings01.string-with-one-placeable";
		Assert.Equal("value: 5", keyedString.Translate(("data", 5)));
	}
	/// <summary>
	/// Tests that a KeyedString can be translated properly when using the default service.
	/// </summary>
	[Fact]
	public void TranslateTest04() {
		Assert.Equal("test", ((KeyedString)"strings01.mockStrings01.sample-string").Translate());
	}
	/// <summary>
	/// Tests that an unloaded KeyedString returns its address when translated when the default service is not set.
	/// </summary>
	[Fact]
	public void TranslateTest05() {
		TranslationService.Default = null;
		Assert.Equal("strings01.mockStrings01.sample-string", ((KeyedString)"strings01.mockStrings01.sample-string").Translate());
	}

	/// <summary>
	/// Tests that a KeyedString gets implicitly cast to its translated string.
	/// </summary>
	[Fact]
	public void ImplicitStringCastTest01() {
		KeyedString keyedString = (KeyedString)"strings01.mockStrings01.sample-string";
		string s = keyedString;

		Assert.Equal("test", s);
	}

	/// <summary>
	/// Tests that the ToString method returns the translated string.
	/// </summary>
	[Fact]
	public void ToStringTest01() {
		KeyedString keyedString = (KeyedString)"strings01.mockStrings01.sample-string";
		Assert.Equal("test", keyedString.ToString(service));
	}
	/// <summary>
	/// Tests that the ToString method returns the translated string with parameters replaced.
	/// </summary>
	[Fact]
	public void ToStringTest02() {
		KeyedString keyedString = (KeyedString)"strings01.mockStrings01.string-with-one-placeable";
		Assert.Equal("value: 5", keyedString.ToString(service, ("data", 5)));
	}
	/// <summary>
	/// Tests that the ToString method returns the translated string when using the default service.
	/// </summary>
	[Fact]
	public void ToStringTest03() {
		KeyedString keyedString = (KeyedString)"strings01.mockStrings01.sample-string";
		Assert.Equal("test", keyedString.ToString());
	}
	#endregion

	#region IsLoaded tests
	/// <summary>
	/// Tests that IsLoaded returns false for a string key that does not exist.
	/// </summary>
	[Fact]
	public void IsLoadedTest01() {
		service = new TranslationService(
			new TranslationServiceOptions {
				PrimaryLocale = new CultureInfo("it-IT")
			}
		);
		_ = service.RegisterResource(Path.Combine(mocksDirectory, "strings01"));
		service.Reload();

		Assert.False(((KeyedString)"sample-string").IsLoaded(service));
	}
	/// <summary>
	/// Tests that IsLoaded returns true for a string key that exists.
	/// </summary>
	[Fact]
	public void IsLoadedTest02() {
		Assert.True(((KeyedString)"strings01.mockStrings01.sample-string").IsLoaded(service));
	}
	/// <summary>
	/// Tests that IsLoaded returns false for a string key that does not exist when using the default service, which is not set.
	/// </summary>
	[Fact]
	public void IsLoadedTest03() {
		TranslationService.Default = null;
		Assert.False(((KeyedString)"sample-string").IsLoaded());
	}
	/// <summary>
	/// Tests that IsLoaded returns true for a string key that exists when using the default service.
	/// </summary>
	[Fact]
	public void IsLoadedTest04() {
		Assert.True(((KeyedString)"strings01.mockStrings01.sample-string").IsLoaded());
	}
	#endregion

	#region Equals tests
	/// <summary>
	/// Tests that two KeyedStrings with the same address have the same hash code.
	/// </summary>
	[Fact]
	public void GetHashCodeTest01() {
		KeyedString str1 = (KeyedString)"namespace.key";
		KeyedString str2 = (KeyedString)"namespace.key";

		Assert.Equal(str1.GetHashCode(), str2.GetHashCode());
	}
	/// <summary>
	/// Tests that a non-null KeyedString does not equal null.
	/// </summary>
	[Fact]
	public void EqualsTest01() {
		KeyedString str1 = (KeyedString)"namespace.key";
		KeyedString? str2 = null;

		Assert.False(str1.Equals(str2));
	}
	/// <summary>
	/// Tests that two KeyedStrings with different addresses do not equal each other.
	/// </summary>
	[Fact]
	public void EqualsTest02() {
		KeyedString str1 = (KeyedString)"namespace.key";
		KeyedString str2 = (KeyedString)"other.namespace.key";

		Assert.False(str1.Equals(str2));
	}
	/// <summary>
	/// Tests that a KeyedString does not equal a different address.
	/// </summary>
	[Fact]
	public void EqualsTest03() {
		KeyedString str1 = (KeyedString)"namespace.key";
		string str2 = "other.namespace.key";

		Assert.False(str1.Equals(str2));
	}
	/// <summary>
	/// Tests that a KeyedString does equal its own address.
	/// </summary>
	[Fact]
	public void EqualsTest04() {
		KeyedString str1 = (KeyedString)"namespace.key";
		string str2 = "namespace.key";

		Assert.True(str1.Equals(str2));
	}
	/// <summary>
	/// Tests that two KeyedStrings with the same address are equal to each other.
	/// </summary>
	[Fact]
	public void EqualsTest05() {
		KeyedString str1 = (KeyedString)"namespace.key";
		KeyedString str2 = (KeyedString)"namespace.key";

		Assert.True(str1.Equals(str2));
	}
	/// <summary>
	/// Tests that a KeyedString does not equal an object of a different type.
	/// </summary>
	[Fact]
	public void EqualsTest06() {
		KeyedString str1 = (KeyedString)"namespace.key";
		int str2 = 5;

		Assert.False(str1.Equals(str2));
	}
	#endregion
}
