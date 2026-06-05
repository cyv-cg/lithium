using System;
using System.Collections.Generic;
using System.IO;
using Fluent.Net;
using System.Linq;
using Fluent.Net.RuntimeAst;
using Lithium.Core.Exceptions;
using System.Reflection;
using Lithium.Core;

namespace Lithium.Strings;

internal static class StringManager {
	/// <summary>
	/// A dictionary mapping namespaces to their corresponding Fluent MessageContexts.
	/// </summary>
	private static readonly Dictionary<string, MessageContext> contexts = new Dictionary<string, MessageContext>();

	/// <summary>
	/// Reloads the string contexts by scanning the root directories for Fluent resource files corresponding to the current locale.
	/// This should be called after adding new root directories to ensure that the latest string resources are loaded and available for translation.
	/// </summary>
	/// <remarks>
	/// Automatically called when changing the locale.
	/// </remarks>
	internal static void Reload() {
		contexts.Clear();

		foreach (Assembly assembly in Settings.EmbeddedResources.Keys) {
			Dictionary<string, string> embeddedResources = GetFilesInLocale(assembly);
			// Construct the contexts for each namespace.
			foreach (string @namespace in embeddedResources.Keys) {
				MessageContext? context = BuildContext(assembly, embeddedResources[@namespace]);
				if (context == null) {
					continue;
				}
				contexts[@namespace] = context;
			}
		}

		Dictionary<string, string> resources = GetFilesInLocale();
		// Construct the contexts for each namespace.
		foreach (string @namespace in resources.Keys) {
			contexts[@namespace] = BuildContext(resources[@namespace]);
		}
	}

	/// <summary>
	/// Scans the root directories for Fluent resource files (.ftl) corresponding to the current locale and organizes them by namespace.
	/// The namespace is derived from the file's relative path within the locale directory, allowing for hierarchical organization of strings.
	/// </summary>
	/// <returns>A dictionary mapping namespaces to lists of file paths for the Fluent resource files found.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the StringRootDirectories setting is null.</exception>
	private static Dictionary<string, string> GetFilesInLocale() {
		if (Settings.StringRootDirectories.Count == 0 && Settings.EmbeddedResources.Count == 0) {
			throw new ResourceRootDirectoryMissingException("String");
		}

		Dictionary<string, string> namespaceFileMap = new Dictionary<string, string>();

		foreach (string directory in Settings.StringRootDirectories) {
			// Use directory structure:
			//	root1/
			//	|	en-US/
			//	|	fr-FR/
			//	...
			//	root2/
			//	|	en-US/
			//	|	fr-FR/
			//	...
			string localeDirectory = Path.Combine(directory, Settings.Locale.Name);
			if (!Directory.Exists(localeDirectory)) {
				continue;
			}

			string[] files = Directory.GetFiles(localeDirectory, "*.ftl", SearchOption.AllDirectories);

			foreach (string file in files) {
				// Categorize string keys into namespaces derived from the file structure.
				string @namespace = GetNamespace(directory, Settings.Locale.Name, file);
				// Map files to their namespace.
				namespaceFileMap[@namespace] = file;
			}
		}

		return namespaceFileMap;
	}
	private static Dictionary<string, string> GetFilesInLocale(Assembly assembly) {
		Dictionary<string, string> namespaceFileMap = new Dictionary<string, string>();

		IEnumerable<string> embeddedResources = Settings.EmbeddedResources[assembly];
		foreach (string resource in embeddedResources) {
			List<string> parts = resource.Split(Path.DirectorySeparatorChar).ToList();

			int localeIndex = parts.IndexOf(Settings.Locale.Name);
			if (localeIndex < 0) {
				continue;
			}
			string @namespace = GetNamespace(resource);
			namespaceFileMap[@namespace] = resource;
		}

		return namespaceFileMap;
	}

	/// <summary>
	/// Builds a Fluent MessageContext from the provided Fluent resource files.
	/// Each file is expected to contain messages for the same locale, and the context will be used for translating strings within that locale.
	/// </summary>
	/// <param name="file">A file path to a Fluent resource file (.ftl) to be loaded into the MessageContext.</param>
	/// <returns>A MessageContext containing the messages from the provided Fluent resource files.</returns>
	/// <exception cref="ParseException">Thrown when there is an error parsing any of the provided Fluent resource files.</exception>
	private static MessageContext BuildContext(string file) {
		MessageContext context = CreateContext();
		ParseMessages(ref context, new StreamReader(file));
		return context;
	}
	private static MessageContext? BuildContext(Assembly assembly, string resource) {
		MessageContext context = CreateContext();
		Stream? stream = ResourceLoader.LoadResourceStream(assembly, resource);
		if (stream == null) {
			return null;
		}
		ParseMessages(ref context, new StreamReader(stream));
		return context;
	}

	private static void ParseMessages(ref MessageContext context, StreamReader reader) {
		List<ParseException> errors = (List<ParseException>)context.AddMessages(reader);
		if (errors.Count != 0) {
			throw errors.First();
		}
	}

	private static MessageContext CreateContext() {
		// When not using bidi text, the inserted control characters can be a nuisance.
		// So currently, this will probably not work for bi-directional text.
		MessageContextOptions options = new MessageContextOptions {
			UseIsolating = false
		};
		return new MessageContext(Settings.Locale.Name, options);
	}

	/// <summary>
	/// Attempts to retrieve the MessageContext and Message corresponding to the provided string key.
	/// </summary>
	/// <param name="address">The key of the string to retrieve, including its namespace (e.g. root.namespace.category.string-key).</param>
	/// <param name="context">The MessageContext containing the string key if found; otherwise, null.</param>
	/// <param name="message">The Message associated with the string key if found; otherwise, null.</param>
	/// <returns>Whether both the MessageContext and Message were successfully retrieved.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the address is an empty string.</exception>
	internal static bool TryGetMessage(string address, out MessageContext? context, out Message? message) {
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
		string[] parts = address.Split(Settings.STRING_NAMESPACE_SEPARATOR);
		if (parts.Length == 0) {
			return (string.Empty, address);
		}
		string @namespace = string.Join(Settings.STRING_NAMESPACE_SEPARATOR, parts.Take(parts.Length - 1));
		string key = parts.Last();
		return (@namespace, key);
	}

	/// <summary>
	/// Categorize string keys into namespaces derived from the file structure.
	/// This will allow multiple root locations to both have a string with the same name while keeping them distinct.
	/// Fluent does not inherently support namespaces, so this is a way of handling them externally.
	/// </summary>
	/// <param name="rootDirectory">The root directory being scanned for Fluent resource files.</param>
	/// <param name="locale">The current locale code.</param>
	/// <param name="fileName">The full file path of the Fluent resource file.</param>
	/// <returns>The namespace derived from the file's relative path within the locale directory as a dot-delimited string (e.g. root.namespace.directory.strings).</returns>
	/// <example>
	/// Inputs:
	/// 	rootDirectory = .../root/
	/// 	locale = en-US
	/// 	fileName = .../root/en-US/namespace/directory/strings.ftl
	/// Output:
	/// 	root.namespace.directory.strings
	/// </example>
	internal static string GetNamespace(string rootDirectory, string locale, string fileName) {
		// '.../root/en-US/'
		string localeDirectory = Path.Combine(rootDirectory, locale);
		// '.../root/en-US/namespace/directory/'
		string fileDirectory = Path.GetDirectoryName(fileName)!;
		// 'root'
		string rootNamespace = Path.GetFileName(rootDirectory.TrimEnd(Path.DirectorySeparatorChar))!;
		// '/namespace/directory/'
		string relativePath = localeDirectory.Equals(fileDirectory) ? string.Empty : Path.GetRelativePath(localeDirectory, fileDirectory);
		// 'root/namespace/directory/strings'
		string namespacePath = Path.Combine(rootNamespace, relativePath, Path.GetFileNameWithoutExtension(fileName));
		// 'root.namespace.directory.strings'
		string @namespace = namespacePath.Replace(Path.DirectorySeparatorChar, Settings.STRING_NAMESPACE_SEPARATOR).TrimStart(Settings.STRING_NAMESPACE_SEPARATOR);

		return @namespace;
	}
	internal static string GetNamespace(string fileName) {
		List<string> parts = fileName.Split(Path.DirectorySeparatorChar).ToList();

		int localeIndex = parts.IndexOf(Settings.Locale.Name);

		string rootDirectory = parts[localeIndex - 1];
		parts.RemoveRange(0, localeIndex + 1);

		string name = Path.GetFileNameWithoutExtension(fileName);
		parts.RemoveAt(parts.Count - 1);

		if (parts.Count > 0) {
			return $"{rootDirectory}.{string.Join('.', parts)}.{name}";
		}
		else {
			return $"{rootDirectory}.{name}";
		}
	}
}
