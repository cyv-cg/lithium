namespace Lithium.Core;

/// <summary>
/// Interface for a service that takes in resources.
/// </summary>
/// <typeparam name="T">Resource data type.</typeparam>
public interface IResourceRegistry<T> {
	/// <summary>
	/// Register a given resource.
	/// </summary>
	/// <param name="resource">The resource to add.</param>
	/// <returns>True if the resource was successfully registered.</returns>
	bool RegisterResource(T resource);

	/// <summary>
	/// Convert registered resources into usable data.
	/// </summary>
	void Reload();
}
