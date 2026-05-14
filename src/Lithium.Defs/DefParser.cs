using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Attributes;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;

namespace Lithium.Defs;

public static class DefParser {
	/// <summary>
	/// Collection of references to nested def properties to be resolved after all top-level defs have been loaded.
	/// </summary>
	private static Stack<DefLink> defLinks = new Stack<DefLink>();

	/// <summary>
	/// Initializes the DefParser by loading all XML files from the defined root directory.
	/// </summary>
	/// <exception cref="ResourceRootDirectoryMissingException">Thrown if the root directory for defs has not been set.</exception>
	public static void LoadAll() {
		if (string.IsNullOrEmpty(Settings.DefRootDirectory)) {
			throw new ResourceRootDirectoryMissingException("Def");
		}
		defLinks.Clear();

		IEnumerable<string> defFiles = XmlLoader.GetAllFiles(Settings.DefRootDirectory);
		DefDatabase.Initialize(defFiles);

		if (!Settings.DeferredParsing) {
			Load();
		}
	}
	/// <summary>
	/// Initializes the DefParser by loading defs from a single XML file.
	/// </summary>
	/// <param name="defFile">Absolute path to the XML file to load.</param>
	public static void LoadSingle(string defFile) {
		defLinks.Clear();
		DefDatabase.Initialize(new string[] { defFile });

		if (!Settings.DeferredParsing) {
			Load();
		}
	}
	/// <summary>
	/// Core loading method that parses defs from XML and resolves def links.
	/// </summary>	}

	/// <summary>
	/// Parses out nested def references.
	/// </summary>
	internal static void ResolveDefLinks() {
		// Load nested defs.
		while (defLinks.Count > 0) {
			DefLink link = defLinks.Pop();
			Type defType = link.Field.PropertyType.IsList(out Type? listType) ? listType! : link.Field.PropertyType;

			// Fetch value from loaded defs.
			if (TryLoadDef(link.DefName, defType, out Def? defValue, true)) {
				if (link.ParentList == null) {
					link.Field.SetValue(link.Instance, defValue);
				}
				else {
					link.ParentList.Add(defValue);
				}
			}
			// Parse an unloaded def.
			else {
				defValue = ParseDef(DefDatabase.LoadXml(link.DefName));
				if (link.ParentList == null) {
					link.Field.SetValue(link.Instance, defValue);
				}
				else {
					link.ParentList.Add(defValue);
				}
			}
		}
	}

	/// <summary>
	/// Loads defs from all XML nodes currently stored in the <see cref="DefDatabase"/>.
	/// </summary>
	private static void Load() {
		// Immediately load all XML nodes.
		IEnumerable<XmlNode> defNodes = DefDatabase.GetAllNodes();
		foreach (XmlNode node in defNodes) {
			Def instance = ParseDef(node);
		}

		ResolveDefLinks();
		DefDatabase.PostLoad();
	}

	/// <summary>
	/// Parses a def from an XML node.
	/// </summary>
	/// <param name="node">XML node containing the data for the def.</param>
	/// <returns>Parsed def.</returns>
	internal static Def ParseDef(XmlNode node) {
		Type? defType = TypeChecker.ResolveType(node.Name);
		if (defType == null) {
			throw new UnresolvedTypeException(node.Name);
		}

		string defKey = DefDatabase.GetDefKey(node);
		object defInstance = Activator.CreateInstance(defType)!;

		if (node.Attributes != null) {
			// Load the "Root" def, if present.
			XmlAttribute? rootAttr = node.Attributes[Constants.DEF_PARENT_ATTR];
			if (rootAttr != null) {
				if (rootAttr.Value == defKey) {
					throw new DefParentInvalidException(defKey, defType, defKey, defType);
				}
				else {
					if (TryLoadDef(rootAttr.Value, defType, out Def? loadedDef)) {
						foreach (PropertyInfo prop in loadedDef!.GetType().GetProperties(TypeChecker.DEF_PROP_BINDINGS)) {
							prop.SetValue(defInstance, prop.GetValue(loadedDef));
						}
					}
					else {
						// Validate the types match.
						Type parentType = TypeChecker.ResolveType(DefDatabase.LoadXml(rootAttr.Value).Name)!;
						if (!parentType.Equals(defType)) {
							throw new DefParentInvalidException(defKey, defType, rootAttr.Value, parentType);
						}
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

		DefDatabase.AddToDB((Def)defInstance);
		return (Def)defInstance;
	}

	/// <summary>
	/// Attempts to load an existing def.
	/// </summary>
	/// <param name="defKey">Key of the def to load.</param>
	/// <param name="defType">Type of the def to load.</param>
	/// <param name="instance">Output variable for the loaded def instance.</param>
	/// <param name="direct">
	/// 	If <see langword="true"/>, only tries to get the pre-loaded def from the database.
	/// 	Otherwise will attempt to dynamically load from XML.
	/// 	Importantly, this should always be used when loading a nested def.
	/// </param>
	/// <returns>True if the def was successfully loaded, false otherwise.</returns>
	private static bool TryLoadDef(string defKey, Type defType, out Def? instance, bool direct = false) {
		object? result;
		if (direct) {
			// Only fetch from defs that have already been loaded.
			result = DefDatabase.LoadDirect(defKey);
			instance = result as Def;
		}
		else {
			// Complicated but necessary way to grab the DefDatabase.Load<T> function.
			MethodInfo loadMethod = typeof(DefDatabase).GetMethod("Load", 1, BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)!;
			MethodInfo genericMethod = loadMethod.MakeGenericMethod(defType);
			// Dynamically load the def with the given key.
			result = genericMethod.Invoke(null, new object[] { defKey });
		}
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
				throw new MissingFieldException(type.ToString(), propNode.Name);
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
							object? entry = Load(node, li, prop, listType!);
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
					object? value = Load(node, propNode, prop, prop.PropertyType);
					if (value != null) {
						prop.SetValue(instance, value);
					}
				}
			}
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
		defLinks.Push(new DefLink(instance, prop, node.InnerText, parent));
	}

	/// <summary>
	/// Loads a value from an XML node based on its type.
	/// Handles primitive types, enums, lists, and custom classes.
	/// </summary>
	/// <param name="node">XML node containing the raw data.</param>
	/// <param name="prop">PropertyInfo of the property being set.</param>
	/// <param name="type"><see cref="Type"/> of the data to read as.</param>
	/// <returns>Data parsed to the given type.</returns>
	private static object? Load(XmlNode defNode, XmlNode node, PropertyInfo prop, Type type) {
		// Load classes with a special constructor.
		if (type.IsSpecialConstructor(out MethodBase? factory)) {
			return LoadFactory(node, factory!);
		}
		// Parse enum values.
		else if (type.IsEnum()) {
			return LoadEnum(defNode, node, type);
		}
		// Special case for System.Type.
		else if (type.IsType()) {
			return LoadType(defNode, node, prop, type);
		}
		// Load sub-classes.
		else if (type.IsNonPrimitive()) {
			return LoadClass(node, type);
		}

		// Convert primitive types.
		return Convert.ChangeType(node.InnerText, type);
	}

	/// <summary>
	/// Loads a class with a special constructor that takes an XmlNode.
	/// </summary>
	/// <param name="node">XML node containing the data.</param>
	/// <param name="factory">Constructor or static factory method to use for loading.</param>
	/// <returns>Instance of the class created by the factory.</returns>
	private static object? LoadFactory(XmlNode node, MethodBase factory) {
		if (factory.IsConstructor) {
			return ((ConstructorInfo)factory).Invoke(new object[] { node });
		}
		return factory.Invoke(null, new object[] { node });
	}
	/// <summary>
	/// Loads an enum value from an XML node.
	/// </summary>
	/// <param name="defNode">XML node containing the def data.</param>
	/// <param name="node">XML node containing the enum value as a string.</param>
	/// <param name="type">Type of the enum to parse.</param>
	/// <returns>Parsed enum value.</returns>
	private static object? LoadEnum(XmlNode defNode, XmlNode node, Type type) {
		if (Enum.TryParse(type, node.InnerText, out object? value)) {
			return value;
		}
		else {
			throw new PropertyLoadException(DefDatabase.GetDefKey(defNode), node.Name, node.InnerText, type);
		}
	}
	/// <summary>
	/// Loads a System.Type value from an XML node, with inheritance enforcement.
	/// </summary>
	/// <param name="defNode">XML node containing the def data.</param>
	/// <param name="node">XML node containing the type name as a string.</param>
	/// <param name="prop">PropertyInfo of the property being set.</param>
	/// <param name="type">Type of the property being set.</param>
	/// <returns>Parsed System.Type value.</returns>
	private static Type? LoadType(XmlNode defNode, XmlNode node, PropertyInfo prop, Type type) {
		Type? targetType = TypeChecker.ResolveType(node.InnerText);
		if (targetType == null) {
			throw new UnresolvedTypeException(node.InnerText);
		}

		object? enforceAttr = prop.GetCustomAttributes(false).FirstOrDefault(
			attr => attr.GetType().IsGenericType &&
			attr.GetType().GetGenericTypeDefinition() == typeof(EnforceInheritance<>)
		);

		if (enforceAttr != null) {
			PropertyInfo parentTypeProperty = enforceAttr.GetType().GetProperty("ParentType")!;
			Type enforcedType = (Type)parentTypeProperty.GetValue(enforceAttr)!;
			if (!enforcedType.IsAssignableFrom(targetType)) {
				throw new DefInheritanceException(DefDatabase.GetDefKey(defNode), prop.Name, targetType, enforcedType);
			}
		}
		return targetType;
	}
	/// <summary>
	/// Loads a non-primitive class by recursively parsing its properties from the XML node.
	/// </summary>
	/// <param name="node">XML node containing the data.</param>
	/// <param name="type">Type of the class to parse.</param>
	/// <returns>Parsed class instance.</returns>
	private static object LoadClass(XmlNode node, Type type) {
		object subClass = Activator.CreateInstance(type)!;
		ParseXmlToClass(node, type, ref subClass);
		return subClass;
	}
}
