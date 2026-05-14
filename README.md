
# cyv.Lithium

Lithium is a framework for dynamically parsing objects at runtime.
Think of it like an ORM but for a filesystem instead of a database.

Lithium aims to be a simple method of defining instance objects outside of source code in a way that new objects can be added and loading without requiring a recompile.
To this end, objects are defined in XML definitions ("defs") to be parsed and loaded at runtime.

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
		<Label>My Def's name</Label>
		<MyInteger>1</MyInteger>
	</MyNamespace.MyCustomDef>

	<MyNamespace.MyCustomDef2>
		<Key>My_Second_Def_Key</Key>
		<Label>My other Def's name</Label>
		<MyInteger>2</MyInteger>
		<MyDouble>2.5</MyDouble>
	</MyNamespace.MyCustomDef2>
</Defs>
```

### Usage

To convert the plain XML into a usable object, first declare where to look for the files, and then they will be ready to access.

<!--TODO: the myDef.ToString() example below is not currently how that functions -->
```C#
using Lithium.Defs;

namespace MyNamespace;

public static void Main(string[] args) {
	// Adds "C:\Documents\project\defs" as a source for def XML files.
	Settings.AddDefRootDirectory("C:\Documents\project\defs");
	// Initialize defs from every discovered file.
	DefParser.LoadAll();

	MyCustomDef? myDef = DefDatabase.Load<MyCustomDef>("My_Def_Key");
	Console.WriteLine(myDef); // "My Def's name"
	Console.WriteLine(myDef.MyInteger); // "1"

	MyCustomDef2? myDef = DefDatabase.Load<MyCustomDef>("My_Second_Def_Key");
	Console.WriteLine(myDef); // "My other Def's name"
	Console.WriteLine(myDef.MyInteger); // "2"
	Console.WriteLine(myDef.MyDouble); // "2.5"
}
```


---
<small>
Copyright (C) 2026 Chris Grassi <br><br>
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version. <br><br>
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details. <br><br>
You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
</small>
