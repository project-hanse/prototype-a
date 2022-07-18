using System;
using System.Collections.Generic;

namespace PipelineService.Models.Pipeline.Execution
{
	/// <summary>
	/// An object that stores the execution of a pipeline.
	/// </summary>
	public record PipelineExecutionRecord : BasePersistentModel
	{
		public Guid PipelineId { get; set; }

		public DateTime StartedOn { get; set; }

		public DateTime CompletedOn { get; set; }

		public IList<OperationExecutionRecord> ToBeExecuted { get; } = new List<OperationExecutionRecord>();

		public IList<OperationExecutionRecord> InExecution { get; } = new List<OperationExecutionRecord>();

		public IList<OperationExecutionRecord> Executed { get; } = new List<OperationExecutionRecord>();

		public IList<OperationExecutionRecord> Failed { get; } = new List<OperationExecutionRecord>();
		public bool IsCompleted => ToBeExecuted.Count == 0 && InExecution.Count == 0;
		public bool Successful => IsCompleted && Failed.Count == 0;
	}
}
