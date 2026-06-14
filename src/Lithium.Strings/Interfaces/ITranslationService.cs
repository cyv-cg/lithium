using System.Collections.Generic;
using System.Reflection;

using StringArgument = (string key, object value);

namespace Lithium.Strings;

public interface ITranslationService {
	public void Reload();
	public string Translate(string key, params StringArgument[] args);
	public IEnumerable<string> GetAllStringAddresses();

	public bool RegisterResource(string directory);
	public bool RegisterResource(Assembly assembly);
}
