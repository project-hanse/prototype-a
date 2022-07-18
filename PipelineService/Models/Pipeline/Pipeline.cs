using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineService.Models.Pipeline
{
	[Table(nameof(Pipeline))]
	public record Pipeline : BasePersistentModel
	{
		/// <summary>
		/// The first (root) node in the pipeline.
		/// </summary>
		[Column("HAS_ROOT")]
		public IList<Operation> Root { get; set; } = new List<Operation>();

		/// <summary>
		/// The pipeline's name.
		/// </summary>
		public string Name { get; set; }

		public string UserIdentifier { get; set; }

		public DateTime? LastRunStart { get; set; }

		public DateTime? LastRunSuccess { get; set; }

		public DateTime? LastRunFailure { get; set; }
	}
}
