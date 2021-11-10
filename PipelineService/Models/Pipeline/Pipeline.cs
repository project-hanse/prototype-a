using System.Collections.Generic;

namespace PipelineService.Models.Pipeline
{
    public record Pipeline : BasePersistentModel
    {
        /// <summary>
        /// The first (root) node in the pipeline.
        /// </summary>
        public IList<Node> Root { get; set; } = new List<Node>();

        /// <summary>
        /// The pipeline's name.
        /// </summary>
        public string Name { get; set; }
    }
}