using System;
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

	private static void Setup() {
		Settings.Reset();
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings01"));
		Settings.SetLocale("en-US");
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
		Setup();

		Assert.Equal("test", ((KeyedString)"strings01.mockStrings01.sample-string").Translate());
	}
	/// <summary>
	/// Tests that an unloaded KeyedString returns its address when translated.
	/// </summary>
	[Fact]
	public void TranslateTest02() {
		Setup();

		Assert.Equal("key-that-does-not-exist", ((KeyedString)"key-that-does-not-exist").Translate());
	}
	/// <summary>
	/// Tests that a KeyedString gets implicityly cast to its translated string.
	/// </summary>
	[Fact]
	public void ImplicitStringCastTest01() {
		Setup();

		Assert.Equal("test", (KeyedString)"strings01.mockStrings01.sample-string");
	}

	/// <summary>
	/// Tests that the ToString method returns the translated string.
	/// </summary>
	[Fact]
	public void ToStringTest01() {
		Setup();

		KeyedString keyedString = (KeyedString)"strings01.mockStrings01.sample-string";
		Assert.Equal("test", keyedString.ToString());
	}
	/// <summary>
	/// Tests that the ToString method returns the translated string with parameters replaced.
	/// </summary>
	[Fact]
	public void ToStringTest02() {
		Setup();

		KeyedString keyedString = (KeyedString)"strings01.mockStrings01.string-with-one-placeable";
		Assert.Equal("value: 5", keyedString.ToString(("data", 5)));
	}
	#endregion

	#region IsLoaded tests
	/// <summary>
	/// Tests that IsLoaded returns false for a string key that does not exist.
	/// </summary>
	[Fact]
	public void IsLoadedTest01() {
		Settings.SetLocale("it-IT");
		Assert.False(((KeyedString)"sample-string").IsLoaded());
	}
	/// <summary>
	/// Tests that IsLoaded returns true for a string key that exists.
	/// </summary>
	[Fact]
	public void IsLoadedTest02() {
		Setup();
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
