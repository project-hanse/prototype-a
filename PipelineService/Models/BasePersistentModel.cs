using System;
using System.ComponentModel.DataAnnotations;

namespace PipelineService.Models
{
	public abstract record BasePersistentModel
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

		public DateTime? ChangedOn { get; set; }
	}
}
