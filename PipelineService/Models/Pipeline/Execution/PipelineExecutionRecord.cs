using System;
using System.Collections.Generic;

namespace PipelineService.Models.Pipeline.Execution
{
    /// <summary>
    /// An object that stores the execution of a pipeline.
    /// </summary>
    public record PipelineExecutionRecord : BasePersistentModel
    {
        public Guid PipelineId { get; set; }

        public DateTime StartedOn { get; set; }

        public DateTime CompletedOn { get; set; }

        public IList<BlockExecutionRecord> ToBeExecuted { get; set; } = new List<BlockExecutionRecord>();

        public IList<BlockExecutionRecord> InExecution { get; set; } = new List<BlockExecutionRecord>();

        public IList<BlockExecutionRecord> Executed { get; set; } = new List<BlockExecutionRecord>();
    }
}