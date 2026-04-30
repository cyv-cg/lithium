using System;
using System.Collections.Generic;
using Lithium.Core.Attributes;
using Lithium.Defs;

namespace Lithium.Defs.Tests;

public class MockDef1 : Def {
	public int sampleValue1;
}

public class MockDef2 : Def {
	public required MockDef1 subDef;
}

public class MockDef3 : Def {
	public required List<Def> defList;
}

public class MockDef4 : Def {
	public required FactoryClass1 factoryClass;
}
public class MockDef5 : Def {
	public required FactoryClass2 factoryClass;
}
public class MockDef6 : Def {
	public required FactoryClass3 factoryClass;
}
public class MockDef7 : Def {
	public required FactoryClass4 factoryClass;
}
public class MockDef8 : Def {
	public required FactoryClass5 factoryClass;
}

public class MockDef9 : Def {
	public float primitiveField;
	public MockEnum enumField;
	public Type? typeField;
	public MockDataClass? classField;
	public List<int>? listField;
}
public class MockDef10 : Def {
	[EnforceInheritance<System.IComparable>]
	public required Type typeField;
}
public class MockDef11 : Def {
	public Nullable<MockDataStruct> classField;
}

public enum MockEnum {
	VALUE1,
	VALUE2
}

public class MockDataClass {
	public int value;
}
public struct MockDataStruct {
	public int value;
}
