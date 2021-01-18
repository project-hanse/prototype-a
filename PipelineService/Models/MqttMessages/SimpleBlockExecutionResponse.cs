using System;

namespace PipelineService.Models.MqttMessages
{
    public class SimpleBlockExecutionResponse : BlockExecutionResponse
    {

        /// <summary>
        /// The id of the resulting dataset stored in the Dataset Storage.
        /// </summary>
        public Guid? ResultDatasetId { get; set; }
    }
}