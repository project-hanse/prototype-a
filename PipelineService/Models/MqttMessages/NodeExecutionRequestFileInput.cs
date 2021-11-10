using System.Collections.Generic;

namespace PipelineService.Models.MqttMessages
{
	public class NodeExecutionRequestFileInput : NodeExecutionRequest
	{
		public string InputObjectKey { get; set; }

		public string InputObjectBucket { get; set; }

		/// <summary>
		/// The configuration of the operation. 
		/// </summary>
		public IDictionary<string, string> OperationConfiguration { get; set; } = new Dictionary<string, string>();
	}
}
