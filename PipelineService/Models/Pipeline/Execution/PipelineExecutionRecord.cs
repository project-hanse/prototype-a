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

        public IList<NodeExecutionRecord> ToBeExecuted { get; set; } = new List<NodeExecutionRecord>();

        public IList<NodeExecutionRecord> InExecution { get; } = new List<NodeExecutionRecord>();

        public IList<NodeExecutionRecord> Executed { get; } = new List<NodeExecutionRecord>();

        public IList<NodeExecutionRecord> Failed { get; } = new List<NodeExecutionRecord>();
    }
}