using System;
using System.Collections.Generic;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.Dtos
{
	public class AddOperationResponse : BaseResponse
	{
		public Guid PipelineId { get; set; }
		public Guid OperationId { get; set; }
		public PipelineVisualizationDto PipelineVisualizationDto { get; set; }
		public IList<Dataset> ResultingDatasets { get; set; }
	}
}
