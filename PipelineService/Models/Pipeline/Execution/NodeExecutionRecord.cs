using System;

namespace PipelineService.Models.Pipeline.Execution
{
	public record NodeExecutionRecord : BasePersistentModel
	{
		public Guid PipelineId { get; set; }

		public Guid NodeId { get; set; }

		public string ResultKey { get; set; }

		public string Name { get; set; }

		public int Level { get; set; }

		public DateTime ExecutionCompletedAt { get; set; }

		public DateTime MovedToStatusInExecutionAt { get; set; }
	}
}
