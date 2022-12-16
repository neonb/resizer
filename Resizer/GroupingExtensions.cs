using System.Collections.Generic;
using System.Linq;

namespace Neonb.Resizer;

internal static class GroupingExtensions
{
	public static void Deconstruct<TKey, TElement>(
		this IGrouping<TKey, TElement> grouping,
		out TKey key,
		out IEnumerable<TElement> elements)
		=> (key, elements) = (grouping.Key, grouping);
}
