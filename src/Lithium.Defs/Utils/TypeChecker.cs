using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lithium.Core;
using Lithium.Core.Attributes;
using Lithium.Core.Exceptions;
using Lithium.Defs.Exceptions;

namespace Lithium.Defs;

internal static class TypeChecker {
	/// <summary>
	/// Binding flags used for reflecting on Def fields. This includes public instance fields and also looks up the inheritance hierarchy to include fields from base classes.
	/// </summary>
	internal const BindingFlags DEF_PROP_BINDINGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

	/// <summary>
	/// A collection of all classes that are:
	/// 	1) Public
	/// 	2) Static
	/// 	3) Tagged with <see cref="UseDefOverrideInitializer"/>
	/// </summary>
	private static Lazy<IEnumerable<Type>> FactoryHelperClasses => new Lazy<IEnumerable<Type>>(
		AppDomain.CurrentDomain
			.GetAssemblies()
			.SelectMany(a => a.GetTypes()
				.Where(c =>
					c.IsClass && c.IsPublic && c.IsAbstract && c.IsSealed && c.IsDefined(typeof(UseDefOverrideInitializer), false)
				)
			)
	);
	/// <summary>
	/// Collection of all static methods tagged with <see cref="DefFactory"/> from every class in <see cref="FactoryHelperClasses"/>.
	/// </summary>
	private static Lazy<IEnumerable<MethodInfo>> StaticFactories => new Lazy<IEnumerable<MethodInfo>>(
		FactoryHelperClasses.Value.SelectMany(c => c.GetMethods().Where(m => m.IsDefined(typeof(DefFactory), false)))
	);
	/// <summary>
	/// Map of all external static def factories. The structure is as follows:
	/// <code>
	/// StaticDefFactories[instanceTypeToBeCreated] = {
	/// 	(SomeStaticClassType, InstanceTypeFactory),
	/// 	(AnotherStaticClassType, DifferentInstanceTypeFactory),
	/// 	etc...
	/// }
	/// </code>
	/// </summary>
	private static Lazy<Dictionary<Type, Dictionary<Type, MethodInfo>>> StaticDefFactories {
		get {
			Dictionary<Type, Dictionary<Type, MethodInfo>> factoryMap = new Dictionary<Type, Dictionary<Type, MethodInfo>>();
			// Map each factory to its return type.
			foreach (MethodInfo factory in StaticFactories.Value) {
				// DeclaringType should never be null here because factory is not a global member.
				Type factoryHelperClass = factory.DeclaringType!;
				// Add or append factory.
				if (factoryMap.TryGetValue(factory.ReturnType, out Dictionary<Type, MethodInfo>? subMap)) {
					subMap[factoryHelperClass] = factory;
					continue;
				}
				factoryMap.Add(factory.ReturnType, new Dictionary<Type, MethodInfo> { { factoryHelperClass, factory } });
			}
			return new Lazy<Dictionary<Type, Dictionary<Type, MethodInfo>>>(factoryMap);
		}
	}

	/// <summary>
	/// Attempts to resolve a type by its name. First checks for internal types in the Lithium namespace, then checks all loaded assemblies.
	/// </summary>
	/// <param name="typeName">Name of the type to resolve.</param>
	/// <returns>The resolved type, or null if it could not be found.</returns>
	internal static Type? ResolveType(string typeName) {
		// Check all loaded assemblies for the type.
		Type? defType = AppDomain.CurrentDomain
			.GetAssemblies()
			.Select(a => a.GetType(typeName))
			.FirstOrDefault(t => t != null);

		return defType;
	}

	/// <summary>
	/// Determines if the given type is a Def.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>True if the type is a Def or derived from Def.</returns>
	internal static bool IsDef(this Type type) {
		return typeof(Def).IsAssignableFrom(type);
	}
	/// <summary>
	/// Checks if the type is a list and returns the type of its elements.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <param name="listType">Output variable containing the generic type of the list elements.</param>
	/// <returns>True if the type is a generic list.</returns>
	internal static bool IsList(this Type type, [NotNullWhen(true)] out Type? listType) {
		listType = null;
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
			listType = type.GetGenericArguments()[0];
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks if the type has a special constructor or factory method for Def initialization.
	/// The constructor must have the <see cref="DefConstructor"/> attribute and take a single
	/// <see cref="XmlNode"/> parameter, or the method must have the <see cref="DefFactory"/> attribute
	/// and also take a single <see cref="XmlNode"/> parameter.
	/// If either is found, the type is considered to have a special constructor.
	/// </summary>
	/// <param name="type">Type to check for a factory.</param>
	/// <param name="node">XML node containing the raw data for the property.</param>
	/// <param name="factory">Output variable containing the invokable factory.</param>
	/// <returns>True if the type has a valid override constructor or factory method.</returns>
	/// <exception cref="DefFactoryMissingException">Thrown when the class has the <see cref="UseDefOverrideInitializer"/> attribute but no applicable constructor or factory method.</exception>
	/// <exception cref="DefFactoryReturnTypeException">Thrown when the def factory has the wrong return type.</exception>
	/// <exception cref="DefFactoryConstructorParamsException">Thrown when the def constructor has the wrong parameter list.</exception>
	internal static bool IsSpecialConstructor(this Type type, XmlNode node, [NotNullWhen(true)] out MethodBase? factory) {
		factory = null;

		// Check whether the instance class itself defines its own factory.
		if (type.IsDefined(typeof(UseDefOverrideInitializer), false)) {
			// First look for a constructor.
			factory = type.GetConstructors().FirstOrDefault(c => c.GetCustomAttribute<DefConstructor>() != null);
			// Then look for a factory.
			factory ??= type.GetMethods().FirstOrDefault(m => m.GetCustomAttribute<DefFactory>() != null);

			// If we still can't find thet factory, give up.
			if (factory == null) {
				throw new DefFactoryMissingException(type);
			}

			return ValidateFactoryIOTypes(type, factory);
		}
		// Check if a factory method exists in a separate static class.
		else if (StaticDefFactories.Value.TryGetValue(type, out Dictionary<Type, MethodInfo>? methodInfo)) {
			// Look for the attribute specifying which factory class to use.
			string factoryClass = node.GetAttributeValue(Constants.DEF_FACTORY_ATTR);
			// If the attribute isn't given, use the first applicable factory class.
			if (string.IsNullOrEmpty(factoryClass)) {
				factory = methodInfo.First().Value;
			}
			else {
				// If there *is* a specified factory class, use that.
				Type? parsedFactoryClass = ResolveType(factoryClass);
				if (parsedFactoryClass == null) {
					throw new UnresolvedTypeException(factoryClass);
				}
				factory = methodInfo[parsedFactoryClass];
			}
			return ValidateFactoryIOTypes(type, factory);
		}

		return false;
	}
	private static bool ValidateFactoryIOTypes(Type type, MethodBase factory) {
		// Verify the factory has the appropriate return type.
		Type? factoryReturnType = null;
		if (factory is MethodInfo method) {
			factoryReturnType = method.ReturnType;
		}
		else if (factory is ConstructorInfo ctor) {
			factoryReturnType = ctor.DeclaringType;
		}

		if (factoryReturnType != type) {
			throw new DefFactoryReturnTypeException(type, factoryReturnType);
		}

		// Verify factory parameters match what's expected.
		Type[] paramTypes = factory.GetParameters().Select(p => p.ParameterType).ToArray();
		if (paramTypes.Length != 1 || paramTypes[0] != typeof(XmlNode)) {
			throw new DefFactoryConstructorParamsException(type);
		}

		return true;
	}

	/// <summary>
	/// Checks if the type is a non-primitive class.
	/// Non-primitive classes are those that are not built-in types (like int, string, etc.)
	/// and are not enums. This includes custom classes and structs.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>True if the type is a non-primitive class or struct.</returns>
	internal static bool IsNonPrimitive(this Type type) {
		return type.IsClass && type != typeof(string);
	}
	/// <summary>
	/// Checks if the type is an enum.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>True if the type is an enum.</returns>
	internal static bool IsEnum(this Type type) {
		return type.IsEnum;
	}
	/// <summary>
	/// Checks if the type is <c>System.Type</c>.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns><c>True</c> if the given type is the type is <c>System.Type</c>.</returns>
	internal static bool IsType(this Type type) {
		return type == typeof(Type);
	}
}
