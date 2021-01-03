namespace PipelineService.Models.Pipeline
{
    public class Pipeline : BaseModel
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