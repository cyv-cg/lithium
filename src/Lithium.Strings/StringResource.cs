using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Fluent.Net;
using Lithium.Core;

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
	public StringResource(CultureInfo locale, Assembly assembly, string resourcePath) : this(locale, resourcePath) {
		Assembly = assembly;
	}

	/// <summary>
	/// Calculates string namespace from file path.
	/// </summary>
	/// <returns>Namespace descriptor for storing the resource.</returns>
	/// <exception cref="FormatException">Thrown if the resource path is not in the format <c>'.../root/locale/path/to/resource/file.ftl'</c>.</exception>
	private string GetNamespace() {
		List<string> parts = ResourcePath.Split(Path.DirectorySeparatorChar).ToList();
		int localeIndex = parts.IndexOf(Locale.Name);

		if (localeIndex < 1) {
			throw new FormatException($"{nameof(ResourcePath)} must be in the format '.../root/{Locale.Name}/path/to/resource.ftl': '{ResourcePath}'.");
		}
		string fileName = Path.GetFileNameWithoutExtension(parts.Last());

		parts.RemoveAt(parts.Count - 1);
		parts.RemoveAt(localeIndex);
		parts.RemoveRange(0, localeIndex - 1);

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
