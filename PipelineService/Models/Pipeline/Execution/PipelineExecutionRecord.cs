using System;
using System.Collections.Generic;
using System.Linq;
using PipelineService.Models.Enums;

namespace PipelineService.Models.Pipeline.Execution
{
	/// <summary>
	/// An object that stores the execution of a pipeline.
	/// </summary>
	public record PipelineExecutionRecord : BasePersistentModel
	{
		public Guid PipelineId { get; set; }

		public DateTime StartedOn { get; set; }

		/// <summary>
		/// The time the pipeline execution was completed.
		/// </summary>
		public DateTime? CompletedOn { get; set; }

		public ExecutionStrategy Strategy { get; set; }

		public bool AllowCachingResults { get; set; }

		/// <summary>
		/// The status the pipeline execution was it at the time of completion (fail or success).
		/// </summary>
		public ExecutionStatus? CompletionStatus { get; set; }

		public IList<OperationExecutionRecord> OperationExecutionRecords { get; set; }


		public bool IsCompleted =>
			OperationExecutionRecords.All(o => o.Status is ExecutionStatus.Succeeded or ExecutionStatus.Failed);

		public bool IsSuccessful => OperationExecutionRecords.All(o => o.Status == ExecutionStatus.Succeeded);

		public bool WaitingForOperations => OperationExecutionRecords.Any(o => o.Status == ExecutionStatus.InExecution);
	}
}
