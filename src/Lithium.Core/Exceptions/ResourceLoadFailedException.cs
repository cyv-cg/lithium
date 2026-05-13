using System;

namespace Lithium.Core.Exceptions;

public class ResourceLoadFailedException(string key) : Exception {
	private string key = key;

	public override string Message => $"Failed to load resource '{key}'.";
}
