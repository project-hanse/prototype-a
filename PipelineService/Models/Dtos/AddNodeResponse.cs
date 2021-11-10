using System;

namespace PipelineService.Models.Dtos
{
	public class AddNodeResponse : BaseResponse
	{
		public Guid PipelineId { get; set; }
		public Guid NodeId { get; set; }
		public PipelineVisualizationDto PipelineVisualizationDto { get; set; }
	}
}
