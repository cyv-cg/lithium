using System.Diagnostics.CodeAnalysis;
using System.Text;

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
	/// <param name="errors">If the resource could not be registered, this will contain details about the errors that occurred.</param>
	/// <returns>True if the resource was successfully registered.</returns>
	bool RegisterResource(T resource, [NotNullWhen(false)] out StringBuilder? errors);

	/// <summary>
	/// Convert registered resources into usable data.
	/// </summary>
	void Reload();
}
