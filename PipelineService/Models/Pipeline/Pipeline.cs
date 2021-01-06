namespace PipelineService.Models.Pipeline
{
    public record Pipeline : BasePersistentModel
    {
        /// <summary>
        /// The first (root) block in the pipeline.
        /// </summary>
        public Block Root { get; set; }

        /// <summary>
        /// The pipeline's name.
        /// </summary>
        public string Name { get; set; }
    }
}