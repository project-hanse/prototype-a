using System;

namespace PipelineService.Models.Dtos
{
    public class NodeTupleDoubleInput
    {
        public string DatasetOneHash { get; set; }
        public Guid NodeOneId { get; set; }
        public Guid DatasetTwoId { get; set; }
        public Guid NodeTwoId { get; set; }
        public Guid TargetNodeId { get; set; }
        public string Description { get; set; }
    }
}