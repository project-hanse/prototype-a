using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineService.Models.Pipeline.Execution
{
    public record NodeExecutionRecord : BasePersistentModel
    {
        public Guid PipelineId { get; set; }

        public Guid NodeId { get; set; }

        [NotMapped]
        public Node Node { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public DateTime ExecutionCompletedAt { get; set; }

        public DateTime MovedToStatusInExecutionAt { get; set; }
    }
}