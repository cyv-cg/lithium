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

	public bool Embedded => Assembly != null;

	[SetsRequiredMembers]
	public StringResource(CultureInfo locale, string resourcePath) {
		ResourcePath = resourcePath;
		Locale = locale;
	}
	[SetsRequiredMembers]
	public StringResource(CultureInfo locale, Assembly assembly, string resourcePath) : this(locale, resourcePath) {
		Assembly = assembly;
	}

	public string GetNamespace() {
		List<string> parts = ResourcePath.Split(Path.DirectorySeparatorChar).ToList();
		int localeIndex = parts.IndexOf(Locale.Name);

		if (localeIndex < 1) {
			throw new FormatException($"{nameof(ResourcePath)} must be in the format '.../root/{Locale.Name}/path/to/resource.ftl': '{ResourcePath}'.");
		}
		string fileName = Path.GetFileNameWithoutExtension(parts.Last());

		parts.RemoveAt(parts.Count - 1);
		parts.RemoveAt(localeIndex);
		parts.RemoveRange(0, localeIndex - 1);

		string @namespace = string.Join(Settings.STRING_NAMESPACE_SEPARATOR, parts);
		return string.Join(Settings.STRING_NAMESPACE_SEPARATOR, @namespace, fileName);
	}

	public IEnumerable<string> GetAddresses() {
		string @namespace = GetNamespace();
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

		return LoadEntries(reader, @namespace);
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
		IEnumerable<string> entries = resource.Entries.Select(e => $"{@namespace}{Settings.STRING_NAMESPACE_SEPARATOR}{e.Key}");
		return entries;
	}

	public MessageContext ToMessageContext() {
		MessageContext context = CreateContext();
		StreamReader reader;

		if (Embedded) {
			Stream? stream = ResourceLoader.LoadResourceStream(Assembly!, ResourcePath);
			if (stream == null) {
				return context;
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
	private static MessageContext CreateContext() {
		// When not using bidi text, the inserted control characters can be a nuisance.
		// So currently, this will probably not work for bi-directional text.
		MessageContextOptions options = new MessageContextOptions {
			UseIsolating = false
		};
		return new MessageContext(Settings.Locale.Name, options);
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
			return res.ResourcePath.Equals(ResourcePath);
		}
		return false;
	}
	public override int GetHashCode() {
		return HashCode.Combine(ResourcePath);
	}
}
