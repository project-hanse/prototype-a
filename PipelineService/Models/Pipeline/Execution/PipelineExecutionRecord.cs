using System;

namespace PipelineService.Models.Pipeline.Execution
{
    /// <summary>
    /// An object that stores the execution of a pipeline.
    /// </summary>
    public class PipelineExecutionRecord : BaseModel
    {
        public Guid PipelineId { get; set; }
    }
}