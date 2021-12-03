using System;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.MqttMessages
{
	public class FrontendExecutionNotification : BaseMqttMessage
	{
		public Guid PipelineId { get; set; }
		public Guid ExecutionId { get; set; }
		public Guid OperationId { get; set; }
		public bool Successful { get; set; }
		public int ExecutionTime { get; set; }
		public string ErrorDescription { get; set; }
		public int OperationsExecuted { get; set; }
		public int OperationsInExecution { get; set; }
		public int OperationsToBeExecuted { get; set; }
		public int OperationsFailedToExecute { get; set; }
		public string OperationName { get; set; }
		public DateTime CompletedAt { get; set; }
		public Dataset ResultDataset { get; set; }
		public string ResultDatasetKey { get; set; }
	}
}
