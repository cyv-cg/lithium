using System;
using System.Collections.Generic;
using System.IO;
using Lithium.Strings.Exceptions;
using Xunit;

namespace Lithium.Strings.Tests;

/// <summary>
/// Tests for Lithium.Strings.TranslationService.cs
/// </summary>
public class TranslationServiceTests {
	private static readonly string mocksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__mocks__");

	private static void Setup() {
		Settings.Reset();
		Settings.SetLocale("en-US");
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings01"));
	}

	/// <summary>
	/// Tests that loaded strings translate properly.
	/// </summary>
	[Fact]
	public void TranslateTest01() {
		Setup();
		TranslationService.Reload();

		Assert.Equal("test", "strings01.mockStrings01.sample-string".Translate());
	}
	/// <summary>
	/// Tests that strings in a sub-namespace translate properly.
	/// </summary>
	[Fact]
	public void TranslateTest02() {
		Setup();
		TranslationService.Reload();

		Assert.Equal("namespace test", "strings01.sub.mockStrings02.sample-string".Translate());
	}
	/// <summary>
	/// Tests that parameters are inserted into translated strings.
	/// </summary>
	[Fact]
	public void TranslateTest03() {
		Setup();
		TranslationService.Reload();

		Assert.Equal("value: 5", "strings01.mockStrings01.string-with-one-placeable".Translate(("data", 5)));
	}
	/// <summary>
	/// Tests that multiple parameters are inserted into translated strings.
	/// </summary>
	[Fact]
	public void TranslateTest04() {
		Setup();
		TranslationService.Reload();

		Assert.Equal("value1: 5, value2: 6", "strings01.mockStrings01.string-with-two-placeables".Translate(("data1", 5), ("data2", 6)));
	}
	/// <summary>
	/// Tests that a KeyNotFoundException is thrown when trying to translate a string that does not exist.
	/// </summary>
	[Fact]
	public void TranslateTest06() {
		Setup();
		TranslationService.Reload();

		Exception? ex = Assert.Throws<KeyNotFoundException>(
			() => "key-that-does-not-exist".Translate()
		);
	}
	/// <summary>
	/// Tests that exceptions are thrown when there is an error in loading the translation.
	/// </summary>
	[Fact]
	public void TranslateTest07() {
		Setup();
		TranslationService.Reload();

		Exception? ex = Assert.Throws<StringTranslationException>(
			() => "strings01.mockStrings01.string-with-bad-selector".Translate()
		);
	}
	/// <summary>
	/// Tests that an ArgumentException is thrown when a string parameter key is empty.
	/// </summary>
	[Fact]
	public void TranslateTest08() {
		Setup();
		TranslationService.Reload();

		Exception? ex = Assert.Throws<ArgumentException>(
			() => "strings01.mockStrings01.string-with-one-placeable".Translate(("", 5))
		);
	}
	/// <summary>
	/// Tests that a KeyNotFoundException is thrown when the namespace exists but the key does not.
	/// </summary>
	[Fact]
	public void TranslateTest09() {
		Setup();
		TranslationService.Reload();

		Exception? ex = Assert.Throws<KeyNotFoundException>(
			() => "strings01.sub.mockStrings02.key-that-does-not-exist".Translate()
		);
	}
	/// <summary>
	/// Tests that a KeyNotFoundException is thrown when neither the namespace nor the key exists.
	/// </summary>
	[Fact]
	public void TranslateTest10() {
		Setup();
		TranslationService.Reload();

		Exception? ex = Assert.Throws<KeyNotFoundException>(
			() => "namespace.that.does.not.exist.key-that-does-not-exist".Translate()
		);
	}
	/// <summary>
	/// Tests that changing the locale properly reloads the string contexts and allows for translation in the new locale.
	/// </summary>
	[Fact]
	public void TranslateTest11() {
		Settings.Reset();
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings02"));

		Settings.SetLocale("en-US");
		Assert.Equal("sample", "strings02.mockStrings.test-string".Translate());

		Settings.SetLocale("fr-FR");
		Assert.Equal("exemple", "strings02.mockStrings.test-string".Translate());
	}
	/// <summary>
	/// Test that unicode characters in translations are handled properly.
	/// </summary>
	[Fact]
	public void TranslateTest12() {
		Settings.Reset();
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings02"));

		Settings.SetLocale("ja-JP");
		Assert.Equal("サンプル", "strings02.mockStrings.test-string".Translate());
	}

	/// <summary>
	/// Tests that:
	/// 	1) percentages are calculated for multiple locales; and
	/// 	2) strings that only exist in secondary locales to not contribute to the percentage.
	/// </summary>
	[Fact]
	public void CalculateTranslationCompletionTest01() {
		Setup();

		Dictionary<string, float> rates = TranslationService.CalculateTranslationCompletion();

		Assert.Equal(1, rates["en-US"]);

		float epsilon = 1e-10f;
		Assert.InRange(rates["fr-FR"], (3f / 6) - epsilon, (3f / 6) + epsilon);
	}
	/// <summary>
	/// Tests that percentages are calculated for multiple secondary locales.
	/// </summary>
	[Fact]
	public void CalculateTranslationCompletionTest02() {
		Settings.Reset();
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings02"));

		Dictionary<string, float> rates = TranslationService.CalculateTranslationCompletion();

		Assert.Equal(1, rates["en-US"]);
		Assert.Equal(1, rates["fr-FR"]);
		Assert.Equal(1, rates["ja-JP"]);
	}
	/// <summary>
	/// Tests that when no root directories are set, the primary locale is the only point of data.
	/// </summary>
	[Fact]
	public void CalculateTranslationCompletionTest03() {
		Settings.Reset();
		Dictionary<string, float> rates = TranslationService.CalculateTranslationCompletion();

		Assert.Empty(rates);
	}
	/// <summary>
	/// Tests that percentages are calculated accurately with multiple root directories.
	/// </summary>
	[Fact]
	public void CalculateTranslationCompletionTest04() {
		Setup();
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings02"));

		Dictionary<string, float> rates = TranslationService.CalculateTranslationCompletion();

		Assert.Equal(1, rates["en-US"]);

		float epsilon = 1e-10f;
		Assert.InRange(rates["fr-FR"], (4f / 7) - epsilon, (4f / 7) + epsilon);
		Assert.InRange(rates["ja-JP"], (1f / 7) - epsilon, (1f / 7) + epsilon);
	}

	[Fact]
	public void CalculateTranslationCompletionTest05() {
		Settings.Reset();
		Settings.AddEmbeddedResources(typeof(TranslationServiceTests).Assembly);

		Dictionary<string, float> rates = TranslationService.CalculateTranslationCompletion();

		Assert.Equal(1, rates["en-US"]);
		Assert.Equal(1, rates["fr-FR"]);
	}
	[Fact]
	public void CalculateTranslationCompletionTest06() {
		Settings.Reset();
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings01"));
		Settings.AddStringRootDirectory(Path.Combine(mocksDirectory, "strings02"));
		Settings.AddEmbeddedResources(typeof(TranslationServiceTests).Assembly);

		Dictionary<string, float> rates = TranslationService.CalculateTranslationCompletion();

		Assert.Equal(1, rates["en-US"]);

		float epsilon = 1e-10f;
		Assert.InRange(rates["fr-FR"], (5f / 8) - epsilon, (5f / 8) + epsilon);
		Assert.InRange(rates["ja-JP"], (1f / 8) - epsilon, (1f / 8) + epsilon);
	}
}
