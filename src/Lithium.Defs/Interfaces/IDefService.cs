using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lithium.Defs;

public interface IDefService {
	void Reload();

	public IEnumerable<T> LoadAll<T>() where T : Def;
	bool TryLoadDef<T>(string key, [NotNullWhen(true)] out T? def) where T : Def;
	T? LoadDef<T>(string key) where T : Def;
}
