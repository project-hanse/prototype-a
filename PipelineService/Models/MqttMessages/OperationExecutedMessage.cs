using System;

namespace PipelineService.Models.MqttMessages
{
	public class OperationExecutedMessage : BaseMqttMessage
	{
		/// <summary>
		/// The pipeline's id the node was executed for.
		/// </summary>
		public Guid PipelineId { get; set; }

		/// <summary>
		/// The pipeline execution this node belongs to.
		/// </summary>
		public Guid ExecutionId { get; set; }

		/// <summary>
		/// The node's id that was executed.
		/// </summary>
		public Guid OperationId { get; set; }

		/// <summary>
		/// Indicates if the execution was successful.
		/// </summary>
		public bool Successful { get; set; }

		/// <summary>
		/// A description of the error that occured if the operation could not be executed successfully.
		/// </summary>
		public string ErrorDescription { get; set; }

		/// <summary>
		/// The time (UTC) the execution of this node was started.
		/// </summary>
		public DateTime StartTime { get; set; }

		/// <summary>
		/// The time (UTC) the execution of this node was stopped (either due to an error or completion).
		/// </summary>
		public DateTime StopTime { get; set; }
	}
}
