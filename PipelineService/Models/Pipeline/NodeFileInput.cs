using PipelineService.Helper;

namespace PipelineService.Models.Pipeline
{
    public record NodeFileInput : Node
    {
        public string InputObjectKey { get; set; }

        public string InputObjectBucket { get; set; }

        /// <summary>
        /// The key (id) the dataset produced by this operation is stored as.
        /// TODO: could this be moved to class Node?
        /// </summary>
        /// <remarks>
        /// Typically sha256(InputDataSetId|InputDataSetHash|OperationName|OperationConfiguration).
        /// </remarks>
        public override string ResultKey =>
            HashHelper.ComputeHash(InputObjectKey, InputObjectBucket, Operation, OperationConfiguration);

        public override string IncludeInHash => InputObjectKey + InputObjectBucket;
    }
}