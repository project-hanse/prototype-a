using System;

namespace PipelineService.Models.Pipeline.Execution
{
	public record OperationExecutionRecord : BasePersistentModel
	{
		public Guid PipelineId { get; set; }

		public Guid OperationId { get; set; }

		/// <summary>
		/// The dataset resulting from this execution of the operation.
		/// </summary>
		public Dataset ResultDataset { get; set; }

		public string Name { get; set; }

		public int Level { get; set; }

		public DateTime ExecutionCompletedAt { get; set; }

		public DateTime MovedToStatusInExecutionAt { get; set; }
	}
}
