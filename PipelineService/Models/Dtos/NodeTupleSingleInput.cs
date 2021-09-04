using System;

namespace PipelineService.Models.Dtos
{
    public class NodeTupleSingleInput
    {
        public string DatasetHash { get; set; }
        public Guid NodeId { get; set; }
        public Guid OperationId { get; set; }
        public Guid TargetNodeId { get; set; }
        public Guid TargetOperationId { get; set; }
        public string Description { get; set; }
        public string Operation { get; set; }
        public string TargetOperation { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is NodeTupleSingleInput typed)
            {
                return Equals(typed);
            }

            return false;
        }

        private bool Equals(NodeTupleSingleInput other)
        {
            return DatasetHash == other.DatasetHash && NodeId.Equals(other.NodeId) &&
                   TargetNodeId.Equals(other.TargetNodeId) && Description == other.Description;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DatasetHash, NodeId, TargetNodeId, Description);
        }
    }
}