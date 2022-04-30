using System;
using System.Collections.Generic;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.MqttMessages
{
	public class OperationExecutionMessage : BaseMqttMessage
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
		/// The id of an operation (node) within a pipeline and execution.
		/// </summary>
		public Guid OperationId { get; set; }

		/// <summary>
		/// The operation's id that will be executed.
		/// </summary>
		public Guid WorkerOperationId { get; set; }

		/// <summary>
		/// An identifier of the operation.
		/// Used by generic pandas operation to execute correct function.
		/// </summary>
		public string WorkerOperationIdentifier { get; set; }

		/// <summary>
		/// The dataset used as inputs for this operation.
		/// </summary>
		public IList<Dataset> Inputs { get; set; }

		/// <summary>
		/// The datasets that will be produced by this operation.
		/// </summary>
		public IList<Dataset> Outputs { get; set; }

		/// <summary>
		/// The key the resulting dataset will be stored as.
		/// </summary>
		[Obsolete("Use Output")]
		public string ResultKey { get; set; }

		/// <summary>
		/// The configuration of the operation.
		/// </summary>
		public IDictionary<string, string> OperationConfiguration { get; set; } = new Dictionary<string, string>();
	}
}
