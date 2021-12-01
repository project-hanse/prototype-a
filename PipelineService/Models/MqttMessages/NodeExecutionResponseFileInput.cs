namespace PipelineService.Models.MqttMessages
{
	public class OperationExecutedMessageNoInput : OperationExecutedMessage
	{
		private string DatasetProducingHash { get; set; }
	}
}
