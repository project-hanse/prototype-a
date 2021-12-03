using System;

namespace PipelineService.Models.Dtos
{
	public class AddOperationResponse : BaseResponse
	{
		public Guid PipelineId { get; set; }
		public Guid OperationId { get; set; }
		public PipelineVisualizationDto PipelineVisualizationDto { get; set; }
	}
}
