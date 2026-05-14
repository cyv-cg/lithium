using System;
using System.IO;

namespace Lithium.Defs.Tests;

internal static class Init {
	internal static readonly string defMocksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__mocks__");

	internal static string MockDirectory(byte i) {
		return Path.Combine(defMocksDirectory, $"mockDefs{i:00}");
	}

	internal static void Setup(byte i) {
		Settings.DefRootDirectories?.Clear();
		Settings.AddDefRootDirectory(MockDirectory(i));
		DefParser.LoadAll();
	}
}
