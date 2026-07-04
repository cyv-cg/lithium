using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Fluent.Net;
using Fluent.Net.RuntimeAst;
using Lithium.Core;
using Lithium.Strings.Exceptions;

using StringArgument = (string key, object value);

namespace Lithium.Strings;

/// <summary>
/// Default template service used to translate strings.
/// Uses Mozilla Fluent for localization.
/// (https://firefox-source-docs.mozilla.org/l10n/fluent/index.html)
/// </summary>
public class TranslationService : ITranslationService, IResourceRegistry<string>, IResourceRegistry<Assembly> {
	/// <summary>
	/// Default translation service.
	/// Used when translating a string without specifying a service to use.
	/// </summary>
	public static ITranslationService? Default { get; set; }

	private readonly TranslationServiceOptions options;
	internal readonly HashSet<StringResource> resources = new HashSet<StringResource>();

	/// <summary>
	/// A dictionary mapping namespaces to their corresponding Fluent MessageContexts.
	/// </summary>
	protected readonly Dictionary<string, MessageContext> contexts = new Dictionary<string, MessageContext>();

	/// <summary>
	/// Initializes the service with set options.
	/// </summary>
	/// <param name="options"><see cref="TranslationServiceOptions"/> for configuration.</param>
	public TranslationService(TranslationServiceOptions options) {
		this.options = options;

		Default ??= this;
	}

	/// <summary>
	/// Registers all external string resources in a directory.
	/// The directory should contain subdirectories named after locales (e.g. "en-US", "fr-FR") which in turn contain the Fluent resource files (.ftl).
	/// </summary>
	/// <param name="directory">Directory containing the Fluent resource files.</param>
	/// <returns>True if all Fluent resource files were registered successfully, false if any failed to register.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the directory string is null or empty.</exception>
	/// <exception cref="DirectoryNotFoundException">Thrown if the given directory does not exist.</exception>
	public bool RegisterResource(string directory) {
		if (string.IsNullOrEmpty(directory)) {
			throw new ArgumentNullException(nameof(directory));
		}
		if (!Directory.Exists(directory)) {
			throw new DirectoryNotFoundException(directory);
		}

		string localeDirectory = Path.Combine(directory, options.PrimaryLocale.Name);
		if (!Directory.Exists(localeDirectory)) {
			return false;
		}

		string[] files = Directory.GetFiles(localeDirectory, "*.ftl", SearchOption.AllDirectories);
		foreach (string file in files) {
			StringResource resource = new StringResource(options.PrimaryLocale, file);
			if (!resources.Add(resource)) {
				return false;
			}
		}
		return true;
	}
	/// <summary>
	/// Registers all Fluent resource files (.ftl) embedded in an assembly.
	/// </summary>
	/// <param name="assembly">Assembly containing the embedded resource files.</param>
	/// <remarks>
	/// The logical names of the resource files are expected to be in a particular format, identical to a hierarchical folder structure.
	/// e.g. 'root/locale/path/to/resource.ftl'.
	///
	/// Resources should be embedded as follows:
	///
	/// <code>
	///	&lt;EmbeddedResource Include=".../resources/MyStrings/**/*.ftl"&gt;
	///		&lt;LogicalName&gt;MyStrings@%(RecursiveDir)%(Filename)%(Extension)&lt;/LogicalName&gt;
	///	&lt;/EmbeddedResource&gt;
	/// </code>
	/// </remarks>
	/// <returns>True if all Fluent resource files were registered successfully, false if any failed to register.</returns>
	public bool RegisterResource(Assembly assembly) {
		IEnumerable<string> embeddedResources = ResourceLoader.FetchResources(assembly, ".ftl").Where(r => r.Contains(options.PrimaryLocale.Name));
		if (!embeddedResources.Any()) {
			return false;
		}

		foreach (string resourcePath in embeddedResources) {
			StringResource resource = new StringResource(options.PrimaryLocale, assembly, resourcePath);
			if (!resources.Add(resource)) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Reloads the string contexts by scanning the root directories for Fluent resource files corresponding to the current locale.
	/// This should be called after adding new root directories to ensure that the latest string resources are loaded and available for translation.
	/// </summary>
	/// <remarks>
	/// Automatically called when changing the locale.
	/// </remarks>
	/// <exception cref="InvalidOperationException">Thrown when duplicate string namespaces are added.</exception>
	public void Reload() {
		contexts.Clear();

		foreach (StringResource resource in resources) {
			string @namespace = resource.Namespace;
			MessageContext? context = resource.ToMessageContext();
			if (context == null) {
				continue;
			}
			if (contexts.ContainsKey(@namespace)) {
				throw new InvalidOperationException($"Duplicate string namespace '{@namespace}' defined at resource '{resource}'");
			}
			contexts[@namespace] = context;
		}
	}

	/// <summary>
	/// Gets a collection of all string keys that are currently loaded for the primary locale.
	/// </summary>
	/// <returns>A collection of all string keys that are currently loaded for the primary locale.</returns>
	public IEnumerable<string> GetAllStringKeys() {
		return resources.SelectMany(r => r.GetAddresses());
	}
	/// <summary>
	/// Determine whether a string with the given key is defined as a translatable unit.
	/// </summary>
	/// <param name="address">String key to search for.</param>
	/// <returns>True if the string is loaded.</returns>
	public bool HasMessage(string address) {
		return TryGetMessage(address, out _, out _);
	}

	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="address">The key of the string to translate, including its namespace (e.g. root.namespace.category.string-key).</param>
	/// <param name="args">Tuples where the first item is the placeable name and the second is the value.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	/// <exception cref="KeyNotFoundException">Thrown when the provided key does not exist in the string database.</exception>
	/// <exception cref="StringTranslationException">Thrown when there is an error during translation or interpolation.</exception>
	/// <exception cref="ArgumentException">Thrown when an argument key is null or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	public string Translate(string address, params StringArgument[] args) {
		if (!TryGetMessage(address, out MessageContext? context, out Message? message)) {
			if (options.FallbackService != null) {
				return options.FallbackService.Translate(address, args);
			}

			throw new KeyNotFoundException(address);
		}

		// Translate and interpolate.
		List<FluentError> errors = new List<FluentError>();
		string result = context!.Format(message, FormatArgs(args), errors);
		// Re-throw the errors.
		if (errors.Count != 0) {
			throw new StringTranslationException(errors);
		}

		return result;
	}
	/// <summary>
	/// Translates the string by replacing parameters with the given values.
	/// </summary>
	/// <param name="string">The <see cref="KeyedString"/> to translate.</param>
	/// <param name="args">Tuples where the first item is the placeable name and the second is the value.</param>
	/// <returns>Translated string with parameters replaced.</returns>
	/// <exception cref="KeyNotFoundException">Thrown when the provided key does not exist in the string database.</exception>
	/// <exception cref="StringTranslationException">Thrown when there is an error during translation or interpolation.</exception>
	/// <exception cref="ArgumentException">Thrown when an argument key is null or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	public string Translate(KeyedString @string, params StringArgument[] args) {
		return Translate(@string.Address, args);
	}

	#region Translate helpers
	/// <summary>
	/// Formats the provided arguments into a dictionary for Fluent interpolation.
	/// </summary>
	/// <param name="args">Tuples where the first item is the placeable name and the second is the value.</param>
	/// <returns>A dictionary mapping placeable names to their corresponding values.</returns>
	/// <exception cref="ArgumentException">Thrown when an argument key is null or empty.</exception>
	protected static Dictionary<string, object> FormatArgs(params StringArgument[] args) {
		Dictionary<string, object> argsMap = new Dictionary<string, object>();

		for (int i = 0; i < args.Length; i++) {
			if (string.IsNullOrEmpty(args[i].key)) {
				throw new ArgumentException($"Expected the argument at index {i} to be a non-empty string", nameof(args));
			}
			argsMap.Add(args[i].key, args[i].value);
		}

		return argsMap;
	}

	/// <summary>
	/// Attempts to retrieve the MessageContext and Message corresponding to the provided string key.
	/// </summary>
	/// <param name="address">The key of the string to retrieve, including its namespace (e.g. root.namespace.category.string-key).</param>
	/// <param name="context">The MessageContext containing the string key if found; otherwise, null.</param>
	/// <param name="message">The Message associated with the string key if found; otherwise, null.</param>
	/// <returns>Whether both the MessageContext and Message were successfully retrieved.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	protected bool TryGetMessage(string address, out MessageContext? context, out Message? message) {
		message = null;

		(string @namespace, string key) = ParseAddress(address);

		// Validate the context exists.
		if (!contexts.TryGetValue(@namespace, out context)) {
			return false;
		}
		if (!context.HasMessage(key)) {
			return false;
		}

		message = context.GetMessage(key);
		return true;
	}
	/// <summary>
	/// Parses the provided string address into its namespace and key components.
	/// </summary>
	/// <param name="address">The string address to parse, including its namespace and key (e.g. root.namespace.category.string-key).</param>
	/// <returns>A tuple containing the namespace and key extracted from the address.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	internal static (string @namespace, string key) ParseAddress(string address) {
		if (string.IsNullOrEmpty(address)) {
			throw new ArgumentNullException(nameof(address));
		}
		// String key should be passed in as a namespace followed by the key itself.
		// e.g. root.namespace.category.string-key
		// This will parse out the namespace and key for proper lookup.
		string[] parts = address.Split(Constants.STRING_NAMESPACE_SEPARATOR);
		if (parts.Length == 1) {
			return (string.Empty, address);
		}
		string @namespace = string.Join(Constants.STRING_NAMESPACE_SEPARATOR, parts.Take(parts.Length - 1));
		string key = parts.Last();
		return (@namespace, key);
	}
	#endregion

	/// <summary>
	/// Calculates what percentage another service is translated compared to this one.
	/// Checks what percent of string addresses defined in this service are also defined in the other service.
	/// Does not include addresses present in the other service that are not in this one.
	/// </summary>
	/// <param name="other">ITranslationService to compare against.</param>
	/// <returns>
	/// A value from 0 to 1 representing what percentage of addresses defined in this service are defined in the other.
	/// Returns -1 if this service does not contain any strings.
	/// </returns>
	public float CompareCompletion(ITranslationService other) {
		if (contexts.Count == 0) {
			return -1f;
		}

		IEnumerable<string> theseKeys = GetAllStringKeys();
		IEnumerable<string> otherKeys = other.GetAllStringKeys();

		if (!otherKeys.Any()) {
			return 0f;
		}

		float completion = theseKeys.Count(otherKeys.Contains) / (float)theseKeys.Count();
		return Math.Clamp(completion, 0f, 1f);
	}
}
