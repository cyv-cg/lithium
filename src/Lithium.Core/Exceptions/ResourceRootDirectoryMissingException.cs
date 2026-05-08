using System;

namespace Lithium.Core.Exceptions;

public class ResourceRootDirectoryMissingException(string resourceName) : Exception {
	private string resourceName = resourceName;

	public override string Message => $"{resourceName} root directory has not been set.";
}
