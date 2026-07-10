using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lithium.Core.Attributes;

namespace Lithium.Defs;

/// <summary>
/// A utility class for scraping assemblies for types and methods.
/// </summary>
internal sealed class AssemblyScraper {
	private readonly IEnumerable<Assembly> assemblies;

	private Dictionary<Type, Dictionary<Type, MethodInfo>>? staticDefFactoriesMapCache;

	public AssemblyScraper(IEnumerable<Assembly> assemblies) {
		this.assemblies = assemblies;
	}

	/// <summary>
	/// Attempts to resolve a type by its name. First checks for internal types in the Lithium namespace, then checks all loaded assemblies.
	/// </summary>
	/// <param name="typeName">Name of the type to resolve.</param>
	/// <returns>The resolved type, or null if it could not be found.</returns>
	public Type? ResolveType(string typeName) {
		// Check all loaded assemblies for the type.
		Type? type = assemblies
			.Select(a => a.GetType(typeName))
			.FirstOrDefault(t => t != null);
		return type;
	}

	/// <summary>
	/// Map of all external static def factories. The structure is as follows:
	/// <code>
	/// StaticDefFactories[instanceTypeToBeCreated] = {
	/// 	(SomeStaticClassType, InstanceTypeFactory),
	/// 	(AnotherStaticClassType, DifferentInstanceTypeFactory),
	/// 	etc...
	/// }
	/// </code>
	/// An external Def factory a public static method that is defined in a class that is:
	/// 	1) Public
	/// 	2) Static
	/// 	3) Tagged with <see cref="UseDefOverrideInitializer"/>.
	/// </summary>
	public Dictionary<Type, Dictionary<Type, MethodInfo>> BuildStaticDefFactoriesMap() {
		if (staticDefFactoriesMapCache != null) {
			return staticDefFactoriesMapCache;
		}

		IEnumerable<MethodInfo> factories = assemblies
			.SelectMany(a => a.GetTypes()
				.Where(c =>
					c.IsClass && c.IsPublic && c.IsAbstract && c.IsSealed && c.IsDefined(typeof(UseDefOverrideInitializer), false)
				)
			)
			.SelectMany(c => c.GetMethods()
				.Where(m => m.IsPublic && m.IsStatic && m.IsDefined(typeof(DefFactory), false))
			);

		Dictionary<Type, Dictionary<Type, MethodInfo>> factoryMap = new Dictionary<Type, Dictionary<Type, MethodInfo>>();
		// Map each factory to its return type.
		foreach (MethodInfo factory in factories) {
			// DeclaringType should never be null here because factory is not a global member.
			Type factoryHelperClass = factory.DeclaringType!;
			// Add or append factory.
			if (factoryMap.TryGetValue(factory.ReturnType, out Dictionary<Type, MethodInfo>? subMap)) {
				if (!subMap.TryAdd(factoryHelperClass, factory)) {
					throw new AmbiguousMatchException($"Duplicate factory methods defined for '{factory.ReturnType}' in class '{factoryHelperClass}'.");
				}
				continue;
			}
			factoryMap.Add(factory.ReturnType, new Dictionary<Type, MethodInfo> { { factoryHelperClass, factory } });
		}

		staticDefFactoriesMapCache = new Dictionary<Type, Dictionary<Type, MethodInfo>>(factoryMap);
		return staticDefFactoriesMapCache;
	}
}
