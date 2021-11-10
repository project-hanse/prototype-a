namespace PipelineService.Models.MqttMessages
{
    public class NodeExecutionResponseNoInput : NodeExecutionResponse
    {
        private string DatasetProducingHash { get; set; }
    }
}