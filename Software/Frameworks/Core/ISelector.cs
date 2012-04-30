using System.Collections;

namespace KOControls.Core
{
	public interface ISelector
	{
		IEnumerable Select(object filter);
	}
}
