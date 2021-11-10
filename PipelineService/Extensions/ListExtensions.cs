using System.Collections.Generic;

namespace PipelineService.Extensions
{
	public static class ListExtensions
	{
		public static IList<T> AddAll<T>(this IList<T> list, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				list.Add(item);
			}

			return list;
		}
	}
}
