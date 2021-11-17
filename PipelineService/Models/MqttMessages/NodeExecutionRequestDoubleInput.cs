using System;
using System.Collections.Generic;

namespace PipelineService.Models.MqttMessages
{
	public class NodeExecutionRequestDoubleInput : NodeExecutionRequest
	{
		/// <summary>
		/// The first input dataset the operation will be performed on.
		/// Might be null if the is using a producing hash for identifying the input dataset.
		/// </summary>
		public Guid? InputDataSetOneId { get; set; }

		/// <summary>
		/// The hash value of the node who's output dataset is the first input for this operation.
		/// </summary>
		public string InputDataSetOneHash { get; set; }

		/// <summary>
		/// The second input dataset the operation will be performed on.
		/// Might be null if the is using a producing hash for identifying the input dataset.
		/// </summary>
		public Guid? InputDataSetTwoId { get; set; }

		/// <summary>
		/// The hash value of the node who's output dataset is the second input for this operation.
		/// </summary>
		public string InputDataSetTwoHash { get; set; }

		/// <summary>
		/// The configuration of the operation. 
		/// </summary>
		public IDictionary<string, string> OperationConfiguration { get; set; } = new Dictionary<string, string>();
	}
}
