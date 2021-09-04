using System;

namespace PipelineService.Models.Dtos
{
    public class NodeTupleSingleInput
    {
        public string DatasetHash { get; set; }
        public Guid NodeId { get; set; }
        public Guid TargetNodeId { get; set; }
        public string Description { get; set; }
    }
}