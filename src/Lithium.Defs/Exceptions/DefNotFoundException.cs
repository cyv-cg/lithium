using System;

namespace Lithium.Defs.Exceptions;

public class DefNotFoundException(string key) : Exception {
	private readonly string key = key;

	public override string Message => $"No Def was found with the key '{key}'.";
}
