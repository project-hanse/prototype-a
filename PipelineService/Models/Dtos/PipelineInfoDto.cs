using System;

namespace PipelineService.Models.Dtos
{
	public class PipelineInfoDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime? ChangedOn { get; set; }
		public string UserIdentifier { get; set; }
		public DateTime? LastRunStart { get; set; }
		public DateTime? LastRunSuccess { get; set; }
		public DateTime? LastRunFailure { get; set; }
	}
}
