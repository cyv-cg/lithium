using System.Globalization;

namespace Lithium.Strings;

public class TranslationServiceOptions {
	public CultureInfo PrimaryLocale { get; set; } = new CultureInfo("en-US");
}
