using System;

namespace PipelineService.Models.Pipeline.Execution
{
	public record OperationExecutionRecord : BasePersistentModel
	{
		public Guid PipelineId { get; set; }

		public Guid OperationId { get; set; }

		public Dataset ResultDataset { get; set; }

		[Obsolete("Replaced by ResultDataset")]
		public string ResultKey { get; set; }

		public string Name { get; set; }

		public int Level { get; set; }

		public DateTime ExecutionCompletedAt { get; set; }

		public DateTime MovedToStatusInExecutionAt { get; set; }
	}
}
