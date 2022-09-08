using System;
using System.Collections.Generic;
using PipelineService.Models.Enums;

namespace PipelineService.Models.Pipeline.Execution
{
	public record OperationExecutionRecord : BasePersistentModel
	{
		public Guid PipelineId { get; set; }

		public Guid OperationId { get; set; }

		public ExecutionStatus Status { get; set; } = ExecutionStatus.ToBeExecuted;

		/// <summary>
		/// The datasets resulting from this execution of the operation.
		/// </summary>
		public IList<Dataset> ResultDatasets { get; set; }

		/// <summary>
		/// The hash of the operation state (config, inputs, etc.) at the time the operation was enqueued to be executed.
		/// </summary>
		public string OperationHash { get; set; }

		public string PredecessorsHash { get; set; }

		public string OperationIdentifier { get; set; }

		public int Level { get; set; }

		public DateTime ExecutionCompletedAt { get; set; }

		public DateTime MovedToStatusInExecutionAt { get; set; }

		/// <summary>
		/// Indicates is operations has been executed successfully.
		/// </summary>
		public bool IsSuccessful => Status == ExecutionStatus.Succeeded;

		public Guid PipelineExecutionRecordId { get; set; }
		public PipelineExecutionRecord PipelineExecutionRecord { get; set; }
	}
}
