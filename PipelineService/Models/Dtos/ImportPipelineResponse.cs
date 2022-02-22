using System;

namespace PipelineService.Models.Dtos;

public class ImportPipelineResponse : BaseResponse
{
	public Guid PipelineId { get; set; }
}
