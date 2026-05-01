using System;
using System.IO;
using Lithium.Strings;

namespace Lithium.Defs.Tests;

internal static class Init {
	internal static readonly string defMocksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__mocks__");

	internal static string MockDirectory(byte i) {
		return Path.Combine(defMocksDirectory, $"mockDefs{i.ToString("00")}");
	}

	internal static void SetupStrings() {
		StringParser.SetStringRootDirectory(Path.Combine(defMocksDirectory, "strings"));
		StringParser.LoadAll();
	}
	internal static void SetupDefs(byte i) {
		DefParser.SetDefRootDirectory(MockDirectory(i));
		DefParser.LoadAll();
	}
	internal static void Setup(byte i) {
		SetupStrings();
		SetupDefs(i);
	}
}
