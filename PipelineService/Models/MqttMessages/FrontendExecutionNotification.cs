using System;

namespace PipelineService.Models.MqttMessages
{
    public class FrontendExecutionNotification : BaseMqttMessage
    {
        public Guid PipelineId { get; set; }
        public Guid ExecutionId { get; set; }
        public Guid BlockId { get; set; }
        public bool Successful { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string ErrorDescription { get; set; }
        public int NodesExecuted { get; set; }
        public int NodesInExecution { get; set; }
        public int ToBeExecuted { get; set; }
    }
}