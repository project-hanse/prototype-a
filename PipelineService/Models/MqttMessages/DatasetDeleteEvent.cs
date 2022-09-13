using PipelineService.Models.Pipeline;

namespace PipelineService.Models.MqttMessages;

/// <summary>
/// Indicates that a dataset should be deleted.
/// </summary>
public class DatasetDeleteEvent : BaseMqttMessage
{
	public Dataset Dataset { get; set; }
}
