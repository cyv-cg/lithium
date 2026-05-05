using System;
using System.Collections;
using System.Reflection;

namespace Lithium.Defs;

internal class DefLink {
	public object Instance { get; set; }
	public PropertyInfo Field { get; set; }
	public string DefName { get; set; }
	public IList? ParentList { get; set; }

	public DefLink(object instance, PropertyInfo field, string defName, IList? parentList = null) {
		Instance = instance;
		Field = field;
		DefName = defName;
		ParentList = parentList;
	}
}
