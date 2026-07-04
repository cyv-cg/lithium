using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Fluent.Net;
using Lithium.Core;
using Lithium.Strings.Exceptions;

namespace Lithium.Strings;

internal sealed class StringResource {
	public required string ResourcePath { get; init; }
	public Assembly? Assembly { get; init; }

	public required CultureInfo Locale { get; init; }
	public required string Namespace { get; init; }

	public bool Embedded => Assembly != null;

	/// <summary>
	/// Define a new external string resource.
	/// </summary>
	/// <param name="locale">Locale of the strings.</param>
	/// <param name="resourcePath">Path to the resource.</param>
	/// <exception cref="ArgumentNullException">Thrown if the resourcePath is empty.</exception>
	/// <exception cref="FormatException">Thrown if the resource path is not in the format <c>'.../root/locale/path/to/resource/file.ftl'</c>.</exception>
	[SetsRequiredMembers]
	public StringResource(CultureInfo locale, string resourcePath) {
		if (string.IsNullOrEmpty(resourcePath)) {
			throw new ArgumentNullException(nameof(resourcePath));
		}

		ResourcePath = resourcePath;
		Locale = locale;
		Namespace = GetNamespace();
	}
	/// <summary>
	/// Define a new embedded string resource.
	/// </summary>
	/// <param name="locale">Locale of the strings.</param>
	/// <param name="assembly">Assembly containing the resource.</param>
	/// <param name="resourcePath">Path to the resource.</param>
	/// <exception cref="ArgumentNullException">Thrown if the resourcePath is empty.</exception>
	/// <exception cref="FormatException">Thrown if the resource path is not in the format <c>'.../root/locale/path/to/resource/file.ftl'</c>.</exception>
	[SetsRequiredMembers]
	public StringResource(CultureInfo locale, Assembly assembly, string resourcePath) {
		if (string.IsNullOrEmpty(resourcePath)) {
			throw new ArgumentNullException(nameof(resourcePath));
		}

		ResourcePath = resourcePath;
		Locale = locale;
		Assembly = assembly;
		Namespace = GetNamespace();
	}

	/// <summary>
	/// Parses resource paths into a string namespace.
	/// </summary>
	/// <returns>Namespace descriptor for storing the resource.</returns>
	/// <exception cref="ResourceFormatException">Thrown if the resource path is not in the correct format.</exception>
	private string GetNamespace() {
		if (Embedded) {
			return GetNamespaceEmbedded();
		}
		else {
			return GetNamespaceExternal();
		}
	}
	/// <summary>
	/// Parses embedded resource paths into a string namespace.
	/// </summary>
	/// <returns>Namespace descriptor for storing the resource.</returns>
	/// <exception cref="ResourceFormatException">Thrown if the resource path is not in the correct format. <c>'root@locale/path/to/resource/file.ftl'</c>.</exception>
	private string GetNamespaceEmbedded() {
		// Split path at the root indicator '@'.
		string[] segments = ResourcePath.Split(Constants.EMBEDDED_RESOURCE_ROOT_INDICATOR);

		// If the indicator wasn't found, it must just be a resource file. Return that.
		if (segments.Length == 1) {
			return Path.GetFileNameWithoutExtension(segments[0]);
		}
		// If the path contains multiple root indicators, throw an error.
		else if (segments.Length > 2) {
			throw new ResourceFormatException(ResourcePath, Embedded, Locale.Name);
		}

		// The name of the root will be the string just befor the @.
		string root = segments[0].Split(Path.DirectorySeparatorChar).Last();
		// Everything after the @ is the remainder of the namespace + the file name.
		string path = segments[1];

		List<string> parts = path.Split(Path.DirectorySeparatorChar).ToList();

		// After the @ should be the locale name.
		if (parts.IndexOf(Locale.Name) != 0) {
			throw new ResourceFormatException(ResourcePath, Embedded, Locale.Name);
		}
		// The file name itself is the last element, so split that off.
		string fileName = Path.GetFileNameWithoutExtension(parts.Last());
		// The locale and file name are not included in the namespace.
		parts.RemoveAt(parts.Count - 1);
		parts.RemoveAt(0);

		// parts can be empty if all the file is stored at the root.
		// In that case, string.Join will include an empty segment if we try to include the namespace.
		if (parts.Count == 0) {
			return string.Join(Constants.STRING_NAMESPACE_SEPARATOR, root, fileName);
		}
		string @namespace = string.Join(Constants.STRING_NAMESPACE_SEPARATOR, parts);
		return string.Join(Constants.STRING_NAMESPACE_SEPARATOR, root, @namespace, fileName);
	}
	/// <summary>
	/// Parses external resource paths into a string namespace.
	/// </summary>
	/// <returns>Namespace descriptor for storing the resource.</returns>
	/// <exception cref="ResourceFormatException">Thrown if the resource path is not in the correct format. <c>'root@locale/path/to/resource/file.ftl'</c>.</exception>
	private string GetNamespaceExternal() {
		// Split up the directory names and find where the locale name is.
		List<string> parts = ResourcePath.Split(Path.DirectorySeparatorChar).ToList();
		int localeIndex = parts.IndexOf(Locale.Name);
		// If the locale is either not found or the first element, that's bad.
		// We assume the locale name immediately follows the root name.
		if (localeIndex < 1) {
			throw new ResourceFormatException(ResourcePath, Embedded, Locale.Name);
		}
		// The file name itself is the last element, so get that.
		string fileName = Path.GetFileNameWithoutExtension(parts.Last());
		// The locale, file name, and everything before the root are not included in the namespace.
		parts.RemoveAt(parts.Count - 1);
		parts.RemoveAt(localeIndex);
		parts.RemoveRange(0, localeIndex - 1);

		// This comes out to root.path.to.file-name
		string @namespace = string.Join(Constants.STRING_NAMESPACE_SEPARATOR, parts);
		return string.Join(Constants.STRING_NAMESPACE_SEPARATOR, @namespace, fileName);
	}

	/// <summary>
	/// Gets the address of every string defined in the resource.
	/// </summary>
	/// <returns>List of string addresses.</returns>
	public IEnumerable<string> GetAddresses() {
		StreamReader reader;

		if (Embedded) {
			Stream? stream = ResourceLoader.LoadResourceStream(Assembly!, ResourcePath);
			if (stream == null) {
				return Array.Empty<string>();
			}
			reader = new StreamReader(stream);
		}
		else {
			reader = new StreamReader(ResourcePath);
		}

		return LoadEntries(reader, Namespace);
	}

	/// <summary>
	/// Loads all string addresses in a namespace.
	/// </summary>
	/// <param name="reader"><see cref="StreamReader"/> containing the file contents.</param>
	/// <param name="namespace">String namespace holding the values.</param>
	/// <returns>List of all string addresses in the namespace.</returns>
	private static IEnumerable<string> LoadEntries(StreamReader reader, string @namespace) {
		FluentResource resource = FluentResource.FromReader(reader);
		// Fetch and store each string key.
		IEnumerable<string> entries = resource.Entries.Select(e => $"{@namespace}{Constants.STRING_NAMESPACE_SEPARATOR}{e.Key}");
		return entries;
	}

	/// <summary>
	/// Creates a usable <see cref="MessageContext"/> from the resource contents.
	/// </summary>
	/// <returns><see cref="MessageContext"/> containing all strings defined in the resource.</returns>
	public MessageContext? ToMessageContext() {
		MessageContext context = CreateContext();
		StreamReader reader;

		if (Embedded) {
			Stream? stream = ResourceLoader.LoadResourceStream(Assembly!, ResourcePath);
			if (stream == null) {
				return null;
			}
			reader = new StreamReader(stream);
		}
		else {
			reader = new StreamReader(ResourcePath);
		}

		ParseMessages(ref context, reader);
		return context;
	}

	/// <summary>
	/// Creates an empty <see cref="MessageContext"/> with default settings.
	/// </summary>
	/// <returns>New <see cref="MessageContext"/>.</returns>
	private MessageContext CreateContext() {
		// When not using bidi text, the inserted control characters can be a nuisance.
		// So currently, this will probably not work for bi-directional text.
		MessageContextOptions options = new MessageContextOptions {
			UseIsolating = false
		};
		return new MessageContext(Locale.Name, options);
	}

	/// <summary>
	/// Attempts to parse Fluent messages from a stream.
	/// </summary>
	/// <param name="context">The <see cref="MessageContext"/> to load messages into.</param>
	/// <param name="reader">The <see cref="StreamReader"/> containing the message contents.</param>
	private static void ParseMessages(ref MessageContext context, StreamReader reader) {
		List<ParseException> errors = (List<ParseException>)context.AddMessages(reader);
		if (errors.Count != 0) {
			throw errors.First();
		}
	}

	/// <summary>
	/// Parses the resource into a string.
	/// </summary>
	/// <returns>The assembly name, if the resource is embedded, or the resource path if it's external.</returns>
	public override string ToString() {
		if (Embedded) {
			return Assembly!.GetName().ToString();
		}
		else {
			return ResourcePath;
		}
	}

	public override bool Equals(object? obj) {
		if (obj is StringResource res) {
			return res.GetHashCode() == GetHashCode();
		}
		return false;
	}
	public override int GetHashCode() {
		return HashCode.Combine(ResourcePath, Embedded);
	}
}
