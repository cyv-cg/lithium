using System;
using System.Collections.Generic;
using System.Xml;
using Lithium.Core;

namespace Lithium.Strings;

public static class StringParser {
	private static string? StringRootDirectory { get; set; }

	private const string INITIAL_LOCALE = "en-US";
	internal static string Locale { get; set; } = INITIAL_LOCALE;

	public static void SetStringRootDirectory(string path) {
		StringRootDirectory = path;
	}
	public static void SetLocale(string locale) {
		Locale = locale;
		LoadAll();
	}

	/// <summary>
	/// Initializes the StringParser by loading all XML files from the defined root directory.
	/// </summary>
	public static void LoadAll() {
		if (string.IsNullOrEmpty(StringRootDirectory)) {
			throw new Exception("String root directory has not been set.");
		}
		StringDatabase.Initialize(XmlLoader.GetAllFiles(StringRootDirectory), Locale);
	}

	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="key">Key for the defined <see cref="KeyedString"/>.</param>
	/// <param name="values">String parameters.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	public static string Translate(this string key, params object[] values) {
		KeyedString keyedString = StringDatabase.Load(key);
		if (keyedString == null) {
			return key;
		}

		return keyedString.Translate(values);
	}
}
