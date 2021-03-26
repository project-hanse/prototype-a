using System;

namespace PipelineService.Models.MqttMessages
{
    public abstract class BlockExecutionRequest : BaseMqttMessage
    {
        /// <summary>
        /// The pipeline's id the block was executed for.
        /// </summary>
        public Guid PipelineId { get; set; }

        /// <summary>
        /// The pipeline execution this block belongs to. 
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// The block's id that was executed.
        /// </summary>
        public Guid BlockId { get; set; }

        /// <summary>
        /// The operations id that will be executed.
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// The name of the operation to make messages more readable for humans.
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// The key the resulting dataset will be stored as.
        /// </summary>
        public string ResultKey { get; set; }
    }
}