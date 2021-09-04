using System;

namespace PipelineService.Models.Dtos
{
    public class NodeTupleSingleInput
    {
        public Guid DatasetId { get; set; }
        public Guid NodeId { get; set; }
        public Guid TargetNodeId { get; set; }
    }
}