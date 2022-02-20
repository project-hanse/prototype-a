using System;

namespace PipelineService.Models.Dtos;

public class PipelineExport
{
	public Guid PipelineId { get; set; }
	public string CreatedBy { get; set; }
	public DateTime CreatedOn { get; set; }
	public string OperationData { get; set; }
	public string PipelineData { get; set; }
}
