using System;

namespace PipelineService.Models.MqttMessages
{
    public class FrontendExecutionNotification : BaseMqttMessage
    {
        public Guid PipelineId { get; set; }
        public Guid ExecutionId { get; set; }
        public Guid BlockId { get; set; }
        public bool Successful { get; set; }
        public int ExecutionTime { get; set; }
        public string ErrorDescription { get; set; }
        public int NodesExecuted { get; set; }
        public int NodesInExecution { get; set; }
        public int NodesToBeExecuted { get; set; }
        public int NodesFailedToExecute { get; set; }
        public string OperationName { get; set; }
        public string ResultDatasetKey { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}