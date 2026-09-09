using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lithium.Defs;

/// <summary>
/// Base interface for a service to manage Defs.
/// </summary>
public interface IDefService {
	/// <summary>
	/// Loads all registered Defs matching the specified type. Matches the type exactly.
	/// </summary>
	/// <typeparam name="T">Type of Defs to load.</typeparam>
	/// <returns>Collection of all Defs matching the supplied type.</returns>
	public IEnumerable<T> LoadAll<T>() where T : Def;
	/// <summary>
	/// Loads all registered Defs.
	/// </summary>
	/// <returns>Collection of all Defs registered in the service.</returns>
	public IEnumerable<Def> LoadAll();
	/// <summary>
	/// Attempts to load a Def object from the registry.
	/// </summary>
	/// <param name="key">Def key to load.</param>
	/// <param name="def">The stored Def object.</param>
	/// <typeparam name="T">Type of the Def to load.</typeparam>
	/// <returns>True if the Def could be loaded, false otherwise.</returns>
	public bool TryLoadDef<T>(string key, [NotNullWhen(true)] out T? def) where T : Def;
	/// <summary>
	/// Attempts to load a Def object from the registry.
	/// </summary>
	/// <param name="id">Def ID to load.</param>
	/// <param name="def">The stored Def object.</param>
	/// <typeparam name="T">Type of the Def to load.</typeparam>
	/// <returns>True if the Def could be loaded, false otherwise.</returns>
	public bool TryLoadDef<T>(uint id, [NotNullWhen(true)] out T? def) where T : Def;
	/// <summary>
	/// Loads a Def object from the registry.
	/// </summary>
	/// <param name="key">Def key to load.</param>
	/// <typeparam name="T">Type of the Def to load.</typeparam>
	/// <returns>The stored Def object.</returns>
	public T? LoadDef<T>(string key) where T : Def;
	/// <summary>
	/// Loads a Def object from the registry.
	/// </summary>
	/// <param name="id">Def ID to load.</param>
	/// <typeparam name="T">Type of the Def to load.</typeparam>
	/// <returns>The stored Def object, or null if the Def exists but does not match the supplied type.</returns>
	public T? LoadDef<T>(uint id) where T : Def;
}
