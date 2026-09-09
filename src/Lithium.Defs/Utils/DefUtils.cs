using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Lithium.Defs.Utils;

/// <summary>
/// Utility methods for Defs.
/// </summary>
public static partial class DefUtils {
	/// <summary>
	/// Gets all applicable properties of a Def.
	/// </summary>
	/// <param name="target">The type to get properties from.</param>
	/// <returns>Collection of settable properties.</returns>
	internal static IEnumerable<PropertyInfo> GetDefProps(this Type target) {
		return target.GetProperties(TypeChecker.DEF_PROP_BINDINGS).Where(p => p.SetMethod != null);
	}

	/// <summary>
	/// Determines whether a Def is only a temporary instance by checking if its key contains a <c>^</c> character.
	/// </summary>
	/// <param name="def">The Def instance to check.</param>
	/// <returns>True if the Def is a temporary instance; false otherwise.</returns>
	internal static bool IsTempDef(this Def def) {
		return def.Key.Contains(Constants.TEMP_DEF_INDICATOR);
	}

	/// <summary>
	/// Basic identifier regex just including a hyphen.
	/// </summary>
	[GeneratedRegex(@"^[a-zA-Z@][a-zA-Z0-9\-_]*$")]
	internal static partial Regex DefKeyRegex();
}
