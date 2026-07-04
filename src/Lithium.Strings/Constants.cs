namespace Lithium.Strings;

internal static class Constants {
	/// <summary>
	/// Char delimiter for portions of a string address.
	/// </summary>
	internal const char STRING_NAMESPACE_SEPARATOR = '.';
	/// <summary>
	/// Char designating the root of an embedded string resource.
	/// Used to get around differences in filesystem paths.
	/// </summary>
	internal const char EMBEDDED_RESOURCE_ROOT_INDICATOR = '@';
}
