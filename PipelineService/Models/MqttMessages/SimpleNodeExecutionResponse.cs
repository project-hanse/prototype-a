using System;

namespace PipelineService.Models.MqttMessages
{
    public class SimpleNodeExecutionResponse : NodeExecutionResponse
    {

        /// <summary>
        /// The id of the resulting dataset stored in the Dataset Storage.
        /// </summary>
        public Guid? ResultDatasetId { get; set; }
    }
}