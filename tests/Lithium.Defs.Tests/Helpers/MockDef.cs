using System;
using System.Collections.Generic;
using Lithium.Core.Attributes;
using Lithium.Defs;

namespace Lithium.Defs.Tests;

public record MockDef1 : Def {
	public int SampleValue1 { get; init; }
}

public record MockDef2 : Def {
	public required MockDef1 SubDef { get; init; }
}

public record MockDef3 : Def {
	public required List<Def> DefList { get; init; }
}

public record MockDef4 : Def {
	public required FactoryClass1 FactoryClass { get; init; }
}
public record MockDef5 : Def {
	public required FactoryClass2 FactoryClass { get; init; }
}
public record MockDef6 : Def {
	public required FactoryClass3 FactoryClass { get; init; }
}
public record MockDef7 : Def {
	public required FactoryClass4 FactoryClass { get; init; }
}
public record MockDef8 : Def {
	public required FactoryClass5 FactoryClass { get; init; }
}

public record MockDef9 : Def {
	public float PrimitiveField { get; init; }
	public MockEnum EnumField { get; init; }
	public Type? TypeField { get; init; }
	public MockDataClass? ClassField { get; init; }
	public List<int>? ListField { get; init; }
}
public record MockDef10 : Def {
	[EnforceInheritance<System.IComparable>]
	public required Type TypeField { get; init; }
}
public record MockDef11 : Def {
	public Nullable<MockDataStruct> ClassField { get; init; }
}

public enum MockEnum {
	VALUE1,
	VALUE2
}

public class MockDataClass {
	public int Value { get; set; }
}
public struct MockDataStruct {
	public int Value { get; set; }
}
