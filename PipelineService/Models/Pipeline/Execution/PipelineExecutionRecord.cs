using System;

namespace PipelineService.Models.Pipeline.Execution
{
    /// <summary>
    /// An object that stores the execution of a pipeline.
    /// </summary>
    public record PipelineExecutionRecord : BasePersistentModel
    {
        public Guid PipelineId { get; set; }
    }
}