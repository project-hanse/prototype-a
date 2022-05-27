namespace PipelineService.Models.Dtos;

public class Pagination
{
	/// <summary>
	/// The property the records are sorted by.
	/// </summary>
	public string Sort { get; set; }

	/// <summary>
	/// The sort order.
	/// Is either 'asc', 'desc' or ''.
	/// </summary>
	public string Order { get; set; }

	public int Page { get; set; }

	public int PageSize { get; set; } = 10;
}
