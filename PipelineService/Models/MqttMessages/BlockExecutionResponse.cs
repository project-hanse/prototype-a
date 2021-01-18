using System;

namespace PipelineService.Models.MqttMessages
{
    public abstract class BlockExecutionResponse : BaseMqttMessage
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
        /// Indicates if the execution was successful.
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// The time (UTC) the execution of this block was started.  
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The time (UTC) the execution of this block was stopped (either due to an error or completion).
        /// </summary>
        public DateTime StopTime { get; set; }
    }
}