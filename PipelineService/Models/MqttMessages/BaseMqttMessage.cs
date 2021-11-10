using System;

namespace PipelineService.Models.MqttMessages
{
    /// <summary>
    /// A base class for all MQTT messages sent between services (eg. PipelineDao, BlockWorker).
    /// </summary>
    public abstract class BaseMqttMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}