namespace Lithium;

public class DefServiceOptions {
	/// <summary>
	/// Deferred Parsing will wait for a def to be used before parsing it from XML.
	/// Non-Deferred Parsing will immediately parse all defs at startup.
	/// </summary>
	public bool DeferredLoad { get; set; } = true;
}
