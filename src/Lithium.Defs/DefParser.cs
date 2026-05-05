using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Attributes;
using Lithium.Defs.Exceptions;

namespace Lithium.Defs;

public static class DefParser {
	private static string? DefRootDirectory { get; set; }

	private static Dictionary<Type, List<DefLink>> defLinks = new Dictionary<Type, List<DefLink>>();

	public static void SetDefRootDirectory(string path) {
		DefRootDirectory = path;
	}

	/// <summary>
	/// Initializes the DefParser by loading all XML files from the defined root directory.
	/// </summary>
	public static void LoadAll() {
		if (string.IsNullOrEmpty(DefRootDirectory)) {
			throw new Exception("Def root directory has not been set.");
		}
		defLinks.Clear();

		IEnumerable<string> defFiles = XmlLoader.GetAllFiles(DefRootDirectory);
		DefDatabase.Initialize(defFiles);

		Load();
	}
	/// <summary>
	/// Initializes the DefParser by loading defs from a single XML file.
	/// </summary>
	/// <param name="defFile">Absolute path to the XML file to load.</param>
	public static void LoadSingle(string defFile) {
		defLinks.Clear();
		DefDatabase.Initialize(new string[] { defFile });

		Load();
	}

	private static void Load() {
		IEnumerable<XmlNode> defNodes = DefDatabase.GetAllNodes();
		foreach (XmlNode node in defNodes) {
			ParseDef(node);
		}

		foreach (Type def in defLinks.Keys) {
			List<DefLink> links = defLinks[def];
			foreach (DefLink link in links) {
				if (TryLoadDef(link.DefName, def, out Def? defValue)) {
					if (link.ParentList == null) {
						link.Field.SetValue(link.Instance, defValue);
					}
					else {
						link.ParentList.Add(defValue);
					}
				}
				else {
					throw new Exception($"Failed to load def '{link.DefName}'");
				}
			}
		}
	}

	/// <summary>
	/// Parses a def from an XML node.
	/// </summary>
	/// <param name="node">XML node containing the data for the def.</param>
	/// <returns>Parsed def.</returns>
	private static Def ParseDef(XmlNode node) {
		Type? defType = TypeChecker.ResolveType(node.Name);
		if (defType == null) {
			throw new Exception($"Could not find def class '{node.Name}'.");
		}
		if (!TypeChecker.IsDef(defType)) {
			throw new DefInheritanceException(defType);
		}

		string defKey = DefDatabase.GetDefKey(node);

		if (TryLoadDef(defKey, defType, out Def? def)) {
			return def!;
		}

		object defInstance = Activator.CreateInstance(defType)!;

		if (node.Attributes != null) {
			// Load the "Root" def, if present.
			XmlAttribute? rootAttr = node.Attributes["Root"];
			if (rootAttr != null) {
				if (rootAttr.Value == defKey) {
					throw new Exception($"Def '{defKey}' cannot refer to itself as the root.");
				}
				else {
					Type parentType = TypeChecker.ResolveType(DefDatabase.LoadXml(rootAttr.Value).Name)!;
					if (!parentType.Equals(defType)) {
						throw new Exception($"Def '{defKey}' ({defType}) is attempting to inherit from '{rootAttr.Value}' ({parentType}).");
					}
					if (TryLoadDef(rootAttr.Value, defType, out Def? loadedDef)) {
						defInstance = loadedDef!;
					}
					else {
						// Load the root instance of the def.
						XmlNode rootNode = DefDatabase.LoadXml(rootAttr.Value);
						object rootInstance = ParseDef(rootNode);
						// After that, copy properties from the root instance to the new one.
						// The reason we have to do that in 2 steps is because ParseDef here will return an instance of the root class.
						// Then when trying to set properties that only exist on the child class, it throws an error because the instance is the wrong type.
						foreach (PropertyInfo prop in rootInstance.GetType().GetProperties(TypeChecker.DEF_PROP_BINDINGS)) {
							prop.SetValue(defInstance, prop.GetValue(rootInstance));
						}
					}
				}
			}
		}

		// Load def properties.
		ParseXmlToClass(node, defType, ref defInstance);

		return (Def)defInstance;
	}

	/// <summary>
	/// /// Attempts to load an existing def.
	/// </summary>
	/// <param name="defKey">Key of the def to load.</param>
	/// <param name="defType">Type of the def to load.</param>
	/// <param name="instance">Output variable for the loaded def instance.</param>
	/// <returns>True if the def was successfully loaded, false otherwise.</returns>
	private static bool TryLoadDef(string defKey, Type defType, out Def? instance) {
		MethodInfo loadMethod = typeof(DefDatabase).GetMethod("Load", 1, BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)!;

		MethodInfo genericMethod = loadMethod.MakeGenericMethod(defType);
		object? result = genericMethod.Invoke(null, new object[] { defKey });
		instance = result as Def;

		return instance != null;
	}

	/// <summary>
	/// Parses the XML node into a class instance.
	/// </summary>
	/// <param name="node">XML node containing the data.</param>
	/// <param name="type">Type of the class to parse into.</param>
	/// <param name="instance">Reference to the instance to populate.</param>
	private static void ParseXmlToClass(XmlNode node, Type type, ref object instance) {
		// Check if any required fields are not defined in XML.
		if (!ValidateRequiredFields(node, type, out IEnumerable<PropertyInfo> missingProps)) {
			throw new MissingDefPropException(DefDatabase.GetDefKey(node), missingProps.ToArray());
		}

		foreach (XmlNode propNode in node.ChildNodes) {
			if (propNode.NodeType == XmlNodeType.Comment) {
				continue;
			}

			PropertyInfo? prop = type.GetProperty(propNode.Name, TypeChecker.DEF_PROP_BINDINGS);
			if (prop == null) {
				throw new WarningException($"Property '{propNode.Name}' does not exist on {type}");
			}

			// Load list elements individually.
			if (prop.PropertyType.IsList(out Type? listType)) {
				IList typedList = (Activator.CreateInstance(typeof(List<>).MakeGenericType(listType!)) as IList)!;
				if (propNode.HasChildNodes) {
					foreach (XmlNode li in propNode.ChildNodes) {
						if (li.NodeType == XmlNodeType.Comment) {
							continue;
						}
						if (listType!.IsDef()) {
							SaveDefLink(instance, li, prop, listType!, typedList);
						}
						else {
							object? entry = Load(li, prop, listType!);
							if (entry != null) {
								typedList.Add(entry);
							}
						}
					}
				}
				prop.SetValue(instance, typedList);
			}
			// Load single values.
			else {
				if (prop.PropertyType.IsDef()) {
					SaveDefLink(instance, propNode, prop, prop.PropertyType);
				}
				else {
					object? value = Load(propNode, prop, prop.PropertyType);
					if (value != null) {
						prop.SetValue(instance, value);
					}
				}
			}
		}

		if (type.IsDef() && instance != null) {
			DefDatabase.AddToDB((Def)instance);
		}
	}

	/// <summary>
	/// Validates that all required fields are present in the XML node.
	/// </summary>
	/// <param name="defNode">XML node containing the def data.</param>
	/// <param name="type">Type of the def being loaded.</param>
	/// <param name="missingProps">Output variable containing any missing required fields.</param>
	/// <returns>True if all required fields are present, false otherwise.</returns>
	private static bool ValidateRequiredFields(XmlNode defNode, Type type, out IEnumerable<PropertyInfo> missingProps) {
		missingProps = new List<PropertyInfo>();
		// Look at every field on the type.
		foreach (PropertyInfo prop in type.GetProperties(TypeChecker.DEF_PROP_BINDINGS)) {
			// Reflections does not supply a way to check if the 'required' modifier is added directly.
			// When compiled, required types are given the [RequiredMember] attribute, which we can test for instead.
			// If that attribute isn't there, then it doesn't matter whether that property is defined.
			if (!Attribute.IsDefined(prop, typeof(System.Runtime.CompilerServices.RequiredMemberAttribute))) {
				continue;
			}
			// Try to grab the matching node from the XML.
			XmlNode? propNode = defNode.SelectSingleNode(prop.Name);
			if (propNode == null) {
				// If the node isn't defined, add it to the list.
				missingProps = missingProps.Append(prop);
			}
		}
		// If we didn't find any missing nodes, we're good!
		return !missingProps.Any();
	}

	/// <summary>
	/// Saves a nested def reference for later resolution.
	/// </summary>
	/// <param name="instance">Instance containing the field.</param>
	/// <param name="node">XML node containing the def name.</param>
	/// <param name="prop">PropertyInfo of the property being set.</param>
	/// <param name="propType">Type of the property being set.</param>
	/// <param name="parent">Optional parent list if the def is part of a list.</param>
	private static void SaveDefLink(object instance, XmlNode node, PropertyInfo prop, Type propType, IList? parent = null) {
		DefLink link = new DefLink(instance, prop, node.InnerText, parent);

		if (defLinks.TryGetValue(propType, out List<DefLink>? links)) {
			links.Add(link);
		}
		else {
			defLinks.Add(propType, new List<DefLink>() { link });
		}
	}

	/// <summary>
	/// Loads a value from an XML node based on its type.
	/// Handles primitive types, enums, lists, and custom classes.
	/// </summary>
	/// <param name="node">XML node containing the raw data.</param>
	/// <param name="prop">PropertyInfo of the property being set.</param>
	/// <param name="type"><see cref="Type"/> of the data to read as.</param>
	/// <returns>Data parsed to the given type.</returns>
	private static object? Load(XmlNode node, PropertyInfo prop, Type type) {
		// Load classes with a special constructor.
		if (type.IsSpecialConstructor(out MethodBase? factory)) {
			return LoadFactory(node, factory!);
		}
		// Parse enum values.
		else if (type.IsEnum()) {
			return LoadEnum(node, type);
		}
		// Special case for System.Type.
		else if (type.IsType()) {
			return LoadType(node, prop, type);
		}
		// Load sub-classes.
		else if (type.IsNonPrimitive()) {
			return LoadClass(node, type);
		}

		// Convert primitive types.
		return Convert.ChangeType(node.InnerText, type);
	}

	private static object? LoadFactory(XmlNode node, MethodBase factory) {
		if (factory.IsConstructor) {
			return ((ConstructorInfo)factory).Invoke(new object[] { node });
		}
		return factory.Invoke(null, new object[] { node });
	}
	private static object? LoadEnum(XmlNode node, Type type) {
		if (Enum.TryParse(type, node.InnerText, out object? value)) {
			return value;
		}
		else {
			throw new Exception($"Invalid value for enum {type}: '{node.InnerText}'.");
		}
	}
	private static Type? LoadType(XmlNode node, PropertyInfo prop, Type type) {
		Type? targetType = TypeChecker.ResolveType(node.InnerText);
		if (targetType == null) {
			throw new Exception($"Could not find type '{node.InnerText}'.");
		}

		object? enforceAttr = prop.GetCustomAttributes(false).FirstOrDefault(
			attr => attr.GetType().IsGenericType &&
			attr.GetType().GetGenericTypeDefinition() == typeof(EnforceInheritance<>)
		);

		if (enforceAttr != null) {
			PropertyInfo parentTypeProperty = enforceAttr.GetType().GetProperty("ParentType")!;
			Type enforcedType = (Type)parentTypeProperty.GetValue(enforceAttr)!;
			if (!enforcedType.IsAssignableFrom(targetType)) {
				throw new Exception($"Prop '{prop.Name}': Type '{targetType}' must inherit from '{enforcedType}'.");
			}
		}
		return targetType;
	}
	private static object LoadClass(XmlNode node, Type type) {
		object subClass = Activator.CreateInstance(type)!;
		ParseXmlToClass(node, type, ref subClass);
		return subClass;
	}
}
