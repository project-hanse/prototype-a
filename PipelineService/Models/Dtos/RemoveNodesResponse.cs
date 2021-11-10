using System;

namespace PipelineService.Models.Dtos
{
    public class RemoveNodesResponse : BaseResponse
    {
        public Guid PipelineId { get; set; }
        public PipelineVisualizationDto PipelineVisualizationDto { get; set; }
    }
}