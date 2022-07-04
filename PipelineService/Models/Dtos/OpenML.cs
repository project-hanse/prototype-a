using Newtonsoft.Json;

namespace PipelineService.Models.Dtos;

public class OpenMlTaskResponse
{
	[JsonProperty("task")]
	public OpenMlTask Task { get; set; }
}

public class OpenMlTask
{
	[JsonProperty("task_id")]
	public int? Id { get; set; }

	[JsonProperty("task_name")]
	public string Name { get; set; }

	[JsonProperty("task_type_id")]
	public int TaskTypeId { get; set; }

	[JsonProperty("task_type")]
	public string TaskType { get; set; }
}
