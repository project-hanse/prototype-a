using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineService.Models.Pipeline.Execution
{
    public record BlockExecutionRecord : BasePersistentModel
    {
        public Guid PipelineId { get; set; }

        public Guid BlockId { get; set; }

        [NotMapped]
        public Block Block { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public DateTime ExecutionCompletedAt { get; set; }

        public DateTime MovedToStatusInExecutionAt { get; set; }
    }
}