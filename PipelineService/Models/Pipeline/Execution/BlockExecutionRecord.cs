using System;

namespace PipelineService.Models.Pipeline.Execution
{
    public record BlockExecutionRecord : BasePersistentModel
    {
        public Guid BlockId { get; set; }

        public string Name { get; set; }
    }
}