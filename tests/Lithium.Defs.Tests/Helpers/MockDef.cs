using System;
using System.Collections.Generic;
using Lithium.Core.Attributes;

namespace Lithium.Defs.Tests;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class MockDef1 : Def {
	public int SampleValue1 { get; init; }
}

public class MockDef2 : Def {
	public required MockDef1 SubDef { get; init; }
}

public class MockDef3 : Def {
	public required List<Def> DefList { get; init; }
}

public class MockDef4 : Def {
	public required FactoryClass1 FactoryClass { get; init; }
}
public class MockDef5 : Def {
	public required FactoryClass2 FactoryClass { get; init; }
}
public class MockDef6 : Def {
	public required FactoryClass3 FactoryClass { get; init; }
}
public class MockDef7 : Def {
	public required FactoryClass4 FactoryClass { get; init; }
}
public class MockDef8 : Def {
	public required FactoryClass5 FactoryClass { get; init; }
}

public class MockDef9 : Def {
	public float PrimitiveField { get; init; }
	public MockEnum EnumField { get; init; }
	public Type? TypeField { get; init; }
	public MockDataClass? ClassField { get; init; }
	public List<int>? ListField { get; init; }
}
public class MockDef10 : Def {
	[EnforceInheritance<IComparable>]
	public required Type TypeField { get; init; }
}
public class MockDef11 : Def {
	public MockDataStruct? ClassField { get; init; }
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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
