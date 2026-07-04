using System.Globalization;

namespace Lithium.Strings;

/// <summary>
/// Options for a translation service.
/// </summary>
public class TranslationServiceOptions {
	/// <summary>
	/// The locale for the resources in the service.
	/// </summary>
	public required CultureInfo PrimaryLocale { get; set; } = new CultureInfo("en-US");
	/// <summary>
	/// Service used as a fallback for when a requested string key was not found.
	/// </summary>
	public ITranslationService? FallbackService { get; set; }
}
