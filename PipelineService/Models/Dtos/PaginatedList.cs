using System.Collections.Generic;

namespace PipelineService.Models.Dtos;

public class PaginatedList<T>
{
	public int TotalItems { get; set; }
	public int Page { get; set; }
	public int PageSize { get; set; }
	public List<T> Items { get; set; }
}
