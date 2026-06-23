using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lithium.Core.Attributes;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;

namespace Lithium.Defs;

/// <summary>
/// Utility for loading defs from XML.
/// Use LoadAll or LoadSingle to parse a def.
/// </summary>
internal static class DefParser {
	/// <summary>
	/// Parses a def from an XML node.
	/// </summary>
	/// <param name="node">XML node containing the data for the def.</param>
	/// <returns>Collection of loaded defs where the first element is the def requested and the rest are its dependencies.</returns>
	internal static IEnumerable<Def> ParseDef(this IDefService service, XmlNode node) {
		Type? defType = TypeChecker.ResolveType(node.Name);
		if (defType == null) {
			throw new UnresolvedTypeException(node.Name);
		}

		object defInstance = Activator.CreateInstance(defType)!;
		service.ParseAttributes(ref defInstance, node, defType);

		HashSet<Def> defs = new HashSet<Def>() {
			(Def)defInstance
		};

		// Load def properties.
		Stack<DefLink> links = ParseXmlToClass(ref defInstance, node, defType);
		defs.UnionWith(service.ResolveDefLinks(links));

		return defs;
	}

	/// <summary>
	/// Parses out nested def references.
	/// </summary>
	private static HashSet<Def> ResolveDefLinks(this IDefService service, Stack<DefLink> links) {
		HashSet<Def> newDefs = new HashSet<Def>();

		// Load nested defs.
		while (links.Count > 0) {
			DefLink link = links.Pop();

			// Either fetch value from loaded defs or load a new one.
			if (!service.TryLoadDef(link.DefName, out Def? defValue)) {
				throw new DefNotFoundException(link.DefName);
			}
			ApplyDefLink(link, defValue);
			_ = newDefs.Add(defValue);
		}

		return newDefs;
	}
	/// <summary>
	/// Sets the value of a DefLink to an instance.
	/// </summary>
	/// <param name="link">DefLink containing the data.</param>
	/// <param name="defValue">Def instance to apply the data to.</param>
	private static void ApplyDefLink(DefLink link, Def defValue) {
		if (link.ParentList == null) {
			link.Field.SetValue(link.Instance, defValue);
		}
		else {
			_ = link.ParentList.Add(defValue);
		}
	}

	private static void ParseAttributes(this IDefService service, ref object defInstance, XmlNode defNode, Type defType) {
		if (defNode.Attributes == null) {
			return;
		}

		string defKey = GetDefKey(defNode);

		// Load the "Root" def, if present.
		XmlAttribute? rootAttr = defNode.Attributes[Constants.DEF_PARENT_ATTR];
		if (rootAttr != null) {
			service.ParseRoot(ref defInstance, rootAttr.Value, defKey, defType);
		}
	}
	private static void ParseRoot(this IDefService service, ref object defInstance, string rootKey, string defKey, Type defType) {
		if (rootKey.Equals(defKey)) {
			throw new DefParentInvalidException(defKey, defType, defKey, defType);
		}
		if (!service.TryLoadDef(rootKey, out Def? rootInstance)) {
			throw new DefNotFoundException(rootKey);
		}

		// Validate the types match.
		if (!rootInstance.GetType().Equals(defType)) {
			throw new DefParentInvalidException(defKey, defType, rootKey, rootInstance.GetType());
		}

		foreach (PropertyInfo prop in rootInstance.GetType().GetProperties(TypeChecker.DEF_PROP_BINDINGS)) {
			prop.SetValue(defInstance, prop.GetValue(rootInstance));
		}
	}

	/// <summary>
	/// Parses the XML node into a class instance.
	/// </summary>
	/// <param name="instance">Reference to the instance to populate.</param>
	/// <param name="defNode">XML node containing the data.</param>
	/// <param name="type">Type of the class to parse into.</param>
	private static Stack<DefLink> ParseXmlToClass(ref object instance, XmlNode defNode, Type type) {
		// Check if any required fields are not defined in XML.
		if (!ValidateRequiredFields(defNode, type, out IEnumerable<PropertyInfo> missingProps)) {
			throw new MissingDefPropException(GetDefKey(defNode), missingProps.ToArray());
		}

		Stack<DefLink> links = new Stack<DefLink>();

		foreach (XmlNode propNode in defNode.ChildNodes) {
			if (propNode.NodeType == XmlNodeType.Comment) {
				continue;
			}

			PropertyInfo? prop = type.GetProperty(propNode.Name, TypeChecker.DEF_PROP_BINDINGS);
			if (prop == null) {
				throw new MissingFieldException(type.ToString(), propNode.Name);
			}

			IEnumerable<DefLink> nestedLinks = prop.PropertyType.IsList(out Type? listType)
				// Load list elements individually.
				? ParseList(ref instance, prop, defNode, propNode, listType)
				// Load single values.
				: ParseSingle(ref instance, prop, defNode, propNode);

			foreach (DefLink link in nestedLinks) {
				links.Push(link);
			}
		}

		return links;
	}

	private static IEnumerable<DefLink> ParseList(ref object instance, PropertyInfo prop, XmlNode defNode, XmlNode listNode, Type listType) {
		IList typedList = (Activator.CreateInstance(typeof(List<>).MakeGenericType(listType!)) as IList)!;
		prop.SetValue(instance, typedList);

		Stack<DefLink> links = new Stack<DefLink>();

		if (!listNode.HasChildNodes) {
			return links;
		}

		foreach (XmlNode li in listNode.ChildNodes) {
			if (li.NodeType == XmlNodeType.Comment) {
				continue;
			}
			if (listType.IsDef()) {
				links.Push(new DefLink(instance, prop, li.InnerText, typedList));
				continue;
			}

			object? entry = LoadProperty(defNode, li, prop, listType, out Stack<DefLink> nestedLinks);
			if (entry != null) {
				_ = typedList.Add(entry);
			}

			foreach (DefLink link in nestedLinks) {
				links.Push(link);
			}
		}

		return links.ToList();
	}
	private static Stack<DefLink> ParseSingle(ref object instance, PropertyInfo prop, XmlNode defNode, XmlNode propNode) {
		Stack<DefLink> links = new Stack<DefLink>();

		if (prop.PropertyType.IsDef()) {
			links.Push(new DefLink(instance, prop, propNode.InnerText));
			return links;
		}

		object? value = LoadProperty(defNode, propNode, prop, prop.PropertyType, out Stack<DefLink> nestedLinks);
		if (value != null) {
			prop.SetValue(instance, value);
		}

		foreach (DefLink link in nestedLinks) {
			links.Push(link);
		}

		return links;
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
	/// Loads a value from an XML node based on its type.
	/// Handles primitive types, enums, lists, and custom classes.
	/// </summary>
	/// <param name="defNode">XML node containing the entire def.</param>
	/// <param name="node">XML node containing the raw data for the property.</param>
	/// <param name="prop">PropertyInfo of the property being set.</param>
	/// <param name="type"><see cref="Type"/> of the data to read as.</param>
	/// <returns>Data parsed to the given type.</returns>
	private static object? LoadProperty(XmlNode defNode, XmlNode node, PropertyInfo prop, Type type, out Stack<DefLink> links) {
		links = new Stack<DefLink>();

		// Load classes with a special constructor.
		if (type.IsSpecialConstructor(out MethodBase? factory)) {
			return LoadFactory(node, factory);
		}
		// Parse enum values.
		else if (type.IsEnum()) {
			return LoadEnum(defNode, node, type);
		}
		// Special case for System.Type.
		else if (type.IsType()) {
			return LoadType(defNode, node, prop);
		}
		// Load sub-classes.
		else if (type.IsNonPrimitive()) {
			return LoadClass(node, type, out links);
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
			throw new PropertyLoadException(GetDefKey(defNode), node.Name, node.InnerText, type);
		}
	}
	/// <summary>
	/// Loads a System.Type value from an XML node, with inheritance enforcement.
	/// </summary>
	/// <param name="defNode">XML node containing the def data.</param>
	/// <param name="node">XML node containing the type name as a string.</param>
	/// <param name="prop">PropertyInfo of the property being set.</param>
	/// <returns>Parsed System.Type value.</returns>
	private static Type? LoadType(XmlNode defNode, XmlNode node, PropertyInfo prop) {
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
				throw new DefInheritanceException(GetDefKey(defNode), prop.Name, targetType, enforcedType);
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
	private static object LoadClass(XmlNode node, Type type, out Stack<DefLink> links) {
		object subClass = Activator.CreateInstance(type)!;
		links = ParseXmlToClass(ref subClass, node, type);
		return subClass;
	}

	private static string GetDefKey(XmlNode node) {
		XmlNode? keyNode = node.SelectSingleNode(Constants.DEF_KEY_ELEMENT);
		if (keyNode == null) {
			throw new NodeMissingChildException(node, Constants.DEF_KEY_ELEMENT);
		}
		return keyNode.InnerText;
	}
}
