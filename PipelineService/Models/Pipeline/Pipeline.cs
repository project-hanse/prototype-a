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
		public IList<Node> Root { get; set; } = new List<Node>();

		/// <summary>
		/// The pipeline's name.
		/// </summary>
		public string Name { get; set; }
	}
}
