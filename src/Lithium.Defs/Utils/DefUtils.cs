using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Lithium.Defs;

/// <summary>
/// Utility methods for Defs.
/// </summary>
public static partial class DefUtils {
	/// <summary>
	/// Copies all properties from one Def to another.
	/// </summary>
	/// <param name="source">The Def to copy from.</param>
	/// <param name="target">Reference to the Def to copy to.</param>
	/// <exception cref="ArgumentException">Thrown if the source and target are not the same type.</exception>
	public static void CopyTo(this Def source, ref Def target) {
		if (!target.GetType().Equals(source.GetType())) {
			throw new ArgumentException($"Cannot copy properties from type '{source.GetType()}' to '{target.GetType()}'.", nameof(target));
		}

		PropertyInfo[] props = target.GetType().GetProperties(TypeChecker.DEF_PROP_BINDINGS);
		foreach (PropertyInfo prop in props) {
			prop.SetValue(target, prop.GetValue(source));
		}
	}

	/// <summary>
	/// Basic identifier regex just including a hyphen.
	/// </summary>
	[GeneratedRegex(@"^[a-zA-Z@][a-zA-Z0-9\-_]*$")]
	internal static partial Regex DefKeyRegex();
}
