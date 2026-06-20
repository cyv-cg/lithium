
# cyv.Lithium

cyv.Lithium is a framework for dynamically parsing objects at runtime.
Think of it like an ORM but for a filesystem instead of a database.

cyv.Lithium aims to be a simple and extensible method of defining instance objects outside of source code in a way that new objects can be added and loading without requiring a recompile.
To this end, objects are defined in XML definitions ("defs") to be parsed and loaded at runtime.

## Installation

### NuGet

See on the [NuGet Gallery](https://www.nuget.org/packages/cyv.Lithium).

```bash
dotnet add package cyv.Lithium --version x.y.z
```

### Manual

See the [releases](https://github.com/cyv-cg/lithium/releases) page on GitHub.
Individual packages can be installed by downloading the `.nupkg` to a [local NuGet package feed](https://learn.microsoft.com/en-us/nuget/hosting-packages/local-feeds).

```
C:\LocalNuGet
└── cyv.Lithium.Core.x.y.z.nupkg
```

```bash
dotnet nuget add source C:\LocalNuget --name local \
dotnet nuget locals all --clear \
dotnet restore --no-cache \
dotnet package add cyv.Lithium.Core --version x.y.z --project MyProject.csproj
```

## Getting started

### Basic syntax

The simplest form of a def consists of a <b>Key</b>, a unique identifier, and a <b>Label</b>, a string name for it.

```C#
/// <summary>
/// Root structure for all Defs.
/// </summary>
public record Def {
	/// <summary>
	/// Primary key used to solely define the object.
	/// Must be distinct from all other Defs.
	/// </summary>
	public required string Key { get; init; }
	/// <summary>
	/// String name for the Def.
	/// </summary>
	public required KeyedString Label { get; init; }
}
```

Custom types can be used as a def as well, but all custom types <i>must</i> in some way inherit from this base-level def record.
In that way, these are both valid def objects:

```C#
namespace MyNamespace;

public record MyCustomDef : Def {
	public int MyInteger { get; init; }
}
public record MyCustomDef2 : MyCustomDef {
	public double MyDouble { get; init; }
}
```

These records are parsed from XML at runtime.
Files containing these definitions must contain a root `<Defs>` tag, where the objects themselves are listed as childen.

```xml
<!-- C:\Documents\project\defs\MyDefs.xml -->

<Defs>
	<MyNamespace.MyCustomDef>
		<Key>My_Def_Key</Key>
		<Label>My Def name</Label>
		<MyInteger>1</MyInteger>
	</MyNamespace.MyCustomDef>

	<MyNamespace.MyCustomDef2>
		<Key>My_Second_Def_Key</Key>
		<Label>My other Def name</Label>
		<MyInteger>2</MyInteger>
		<MyDouble>2.5</MyDouble>
	</MyNamespace.MyCustomDef2>
</Defs>
```

### Usage

To convert the plain XML into a usable object, first declare where to look for the files, and then they will be ready to access.

```C#
using Lithium.Defs;

namespace MyNamespace;

public class Program {
	public static void Main(string[] args) {
		// Adds "C:\Documents\project\defs" as a source for def XML files.
		Settings.AddDefRootDirectory("C:\Documents\project\defs");
		// Initialize defs from every discovered file.
		DefDatabase.Initialize();

		MyCustomDef? myDef = DefDatabase.Load<MyCustomDef>("My_Def_Key");
		Console.WriteLine(myDef); // "My Def name"
		Console.WriteLine(myDef.MyInteger); // "1"

		MyCustomDef2? myDef2 = DefDatabase.Load<MyCustomDef2>("My_Second_Def_Key");
		Console.WriteLine(myDef2); // "My other Def name"
		Console.WriteLine(myDef2.MyInteger); // "2"
		Console.WriteLine(myDef2.MyDouble); // "2.5"
	}
}
```

## Strings

While cyv.Lithium is primarily meant for dynamically loading objects, it also bundles in localization support, leveraging [Mozilla Fluent](https://firefox-source-docs.mozilla.org/l10n/fluent/index.html), leveraging [Fluent.Net](https://github.com/blushingpenguin/Fluent.Net/tree/master).

This will not include a general guide on how to use Fluent, see the [Fluent syntax guide](https://projectfluent.org/fluent/guide/index.html) for more on that.

### Introduction

This implementation aims to make integrating Fluent as seamless as possible, with a similar philosophy to the rest of the package.
That means,
1) you define where the files are stored; and
2) strings are dynamically loaded at runtime.

This also wraps additional logic regarding namespaces into Fluent, helping to reduce key collisions in projects with a lot of different strings, and tries to simplify the process of accessing those strings.

### Structure

Fluent strings are stored in `.ftl` files, which are arranged in the following structure:

```
root1/
├── en-US/
│	├── area1/
│	│	├── strings.ftl
|	|	└── more-strings.ftl
│	└── area2/
│		└── yet-more-strings.ftl
├── fr-FR/
│	├── area1/
│	│	├── strings.ftl
|	|	└── more-strings.ftl
│	└── area2/
│		└── yet-more-strings.ftl
├── ...

root2/
├── en-US/
│	├── ...
├── ...
```
Strings for each locale are stored in a folder named after the locale under each root directory.

Each defined string is given an address based on this folder structure. For example, take this file:
```ini
# C:\Documents\project\strings\root1\en-US\area1\strings.ftl
string-key = Text in English (United States).
```
That string would be fully identified as
`root1.area1.strings.string-key`.

### Usage

Translations can be accessed by calling the `Translate()` function on an address string.

```C#
using Lithium.Strings;

namespace MyNamespace;

public class Program {
	public static void Main(string[] args) {
		// Initialize the service.
		TranslationServiceOptions options = new TranslationServiceOptions
		{
			// Change to your preferred locale.
			PrimaryLocale = new CultureInfo("en-US")
		};
		TranslationService service = new TranslationService(options);

		// Adds "C:\Documents\project\strings\root1" as a source for Fluent files.
		service.RegisterResource("C:\Documents\project\strings\root1");
		service.Reload();

		Console.WriteLine(service.Translate("root1.area1.strings.string-key")); // "Text in English (United States)."


		options = new TranslationServiceOptions
		{
			PrimaryLocale = new CultureInfo("fr-FR")
		};
		service = new TranslationService(options);
		service.RegisterResource("C:\Documents\project\strings\root1");
		service.Reload();

		Console.WriteLine(service.Translate("root1.area1.strings.string-key")); // "Texte en français (France)."
	}
}
```

Strings can also be interpolated by passing a list of key-value-pair tuples to the Translate function.
```ini
# C:\Documents\project\strings\root1\en-US\area1\more-strings.ftl
string-with-parameters = value1: { $param1 }, value2: { $param2 }
```
```C#
using Lithium.Strings;

namespace MyNamespace;

public class Program {
	public static void Main(string[] args) {
		TranslationServiceOptions options = new TranslationServiceOptions
		{
			PrimaryLocale = new CultureInfo("en-US")
		};
		TranslationService service = new TranslationService(options);
		service.RegisterResource("C:\Documents\project\strings\root1");
		service.Reload();

		Console.WriteLine(
			service.Translate("root1.area2.yet-more-strings.string-with-parameters",
				("param1", 2.3f),
				("param2", "text")
			)
		); // "value1: 2.3, value2: text"
	}
}
```


#### Usage with Defs

When creating a def in XML, you can use one of these string addresses as the label.

```xml
<!-- C:\Documents\project\defs\MyDefs.xml -->

<Defs>
	<MyNamespace.MyCustomDef>
		<Key>My_Def_Key</Key>
		<Label>root1.area1.strings.my-def-name</Label>
	</MyNamespace.MyCustomDef>
</Defs>
```
```ini
# C:\Documents\project\strings\root1\en-US\area2\yet-more-strings.ftl
my-def-name = My Def name
```
```ini
# C:\Documents\project\strings\root1\fr-FR\area2\yet-more-strings.ftl
my-def-name = Le nom de ma déf
```

The `TranslationService` class comes with a built-in singleton, `TranslationService.Default`. When translating a string without specifying the service, this one is used.

```C#
using Lithium.Defs;
using Lithium.Strings;

namespace MyNamespace;

public class Program {
	public static void Main(string[] args) {
		Lithium.Defs.Settings.AddDefRootDirectory("C:\Documents\project\defs");
		DefDatabase.Initialize();

		TranslationServiceOptions options = new TranslationServiceOptions
		{
			PrimaryLocale = new CultureInfo("en-US")
		};
		// When declaring your first service, it gets saved to TranslationService.Default.
		TranslationService service = new TranslationService(options);
		service.RegisterResource("C:\Documents\project\strings\root1");
		service.Reload();

		MyCustomDef? myDef = DefDatabase.Load<MyCustomDef>("My_Def_Key");
		Console.WriteLine(myDef.Label.Address); // "root1.area2.yet-more-strings.my-def-name"

		// These are all valid ways to translate a string.
		Console.WriteLine(TranslationService.Default.Translate(myDef.Label)); // "My Def name"
		Console.WriteLine(service.Translate(myDef.Label)); // "My Def name"
		Console.WriteLine(myDef.Label.Translate(service)); // "My Def name"
		Console.WriteLine(myDef.Label); // "My Def name"
		Console.WriteLine(myDef); // "My Def name"


		// New services can be declared and used at runtime.

		options = new TranslationServiceOptions
		{
			PrimaryLocale = new CultureInfo("fr-FR")
		};
		TranslationService otherService = new TranslationService(options);
		otherService.RegisterResource("C:\Documents\project\strings\root1");
		otherService.Reload();

		// Creating a second service does not change the default.
		Console.WriteLine(otherService.Translate(myDef)); // "Le nom de ma déf"
		Console.WriteLine(myDef); // "My Def name"
	}
}
```

---
Copyright (C) 2026 Chris Grassi <br><br>
This program is free software: you can redistribute it and/or modify<br>
it under the terms of the GNU General Public License as published by<br>
the Free Software Foundation, either version 3 of the License, or<br>
(at your option) any later version. <br><br>
This program is distributed in the hope that it will be useful,<br>
but WITHOUT ANY WARRANTY; without even the implied warranty of<br>
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the<br>
GNU General Public License for more details. <br><br>
You should have received a copy of the GNU General Public License<br>
along with this program. If not, see https://www.gnu.org/licenses/.
