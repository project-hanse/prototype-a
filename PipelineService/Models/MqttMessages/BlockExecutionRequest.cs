using System;
using System.Collections.Generic;

namespace PipelineService.Models.MqttMessages
{
    public class BlockExecutionRequest : MqttBaseMessage
    {
        public Guid PipelineId { get; set; }

        public Guid BlockId { get; set; }

        public Guid ExecutionId { get; set; }

        public IList<Guid> InputDataSetIds { get; set; } = new List<Guid>();

        public string OperationName { get; set; }
    }
}