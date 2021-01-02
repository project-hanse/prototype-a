using System;

namespace PipelineService.Models.MqttMessages
{
    public class BlockExecutionResponse : MqttBaseMessage
    {
        /// <summary>
        /// The pipeline's id the block was executed for.
        /// </summary>
        public Guid PipelineId { get; set; }

        /// <summary>
        /// The block's id that was executed.
        /// </summary>
        public Guid BlockId { get; set; }

        /// <summary>
        /// Indicates if the execution was successful.
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// The number of milliseconds it took to execute the block.
        /// </summary>
        public int ExecutionTime { get; set; }

        /// <summary>
        /// The id of the resulting dataset stored ini the Dataset Storage.
        /// </summary>
        public Guid ResultDatasetId { get; set; }
    }
}