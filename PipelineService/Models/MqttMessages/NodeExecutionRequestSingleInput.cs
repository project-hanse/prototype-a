using System;
using System.Collections.Generic;

namespace PipelineService.Models.MqttMessages
{
    public class NodeExecutionRequestSingleInput : BlockExecutionRequest
    {
        /// <summary>
        /// The input dataset the operation will be performed on.
        /// Might be null if the is using a producing hash for identifying the input dataset.
        /// </summary>
        public Guid? InputDataSetId { get; set; }

        /// <summary>
        /// The hash value of the block who's output is the input for this operation.
        /// </summary>
        public string InputDataSetHash { get; set; }

        /// <summary>
        /// The configuration of the operation. 
        /// </summary>
        public IDictionary<string, string> OperationConfiguration { get; set; } = new Dictionary<string, string>();
        
    }
}