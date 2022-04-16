using System.Collections.Generic;

namespace PipelineService.Extensions
{
	public static class ListExtensions
	{
		public static ICollection<T> AddAll<T>(this ICollection<T> list, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				list.Add(item);
			}

			return list;
		}
	}
}
