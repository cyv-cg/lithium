using System;

namespace Lithium.Core.Exceptions;

/// <summary>
/// Exceptoin indicating that a resource was attempted to load before adding a location to read from.
/// </summary>
/// <param name="resourceName">
/// Name of the resource type.
/// Not meaningful technically, and only used as a descriptor.
/// </param>
public class ResourceRootDirectoryMissingException(string resourceName) : Exception {
	private readonly string resourceName = resourceName;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message => $"{resourceName} root directory has not been set.";
}
