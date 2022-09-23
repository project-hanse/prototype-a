using System;
using System.ComponentModel.DataAnnotations;
using PipelineService.Models.Enums;

namespace PipelineService.Models.Pipeline.Execution
{
	public record OperationExecutionRecord : BasePersistentModel
	{
		public Guid PipelineId { get; set; }

		public Guid OperationId { get; set; }

		public ExecutionStatus Status { get; set; } = ExecutionStatus.ToBeExecuted;

		/// <summary>
		/// The hash of the operation state (config, inputs, etc.) at the time the operation was enqueued to be executed.
		/// </summary>
		[MaxLength(255)]
		public string OperationHash { get; set; }

		[MaxLength(255)]
		public string PredecessorsHash { get; set; }

		[MaxLength(255)]
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

		/// <summary>
		/// Indicates if the resulting datasets have been newly computed or a cached result already existed.
		/// </summary>
		public bool Cached { get; set; }

		public string ErrorMessage { get; set; }
	}
}
