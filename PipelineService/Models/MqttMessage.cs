using System;

namespace PipelineService.Models
{
    public class MqttMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PipelineId { get; set; }

        public string Message { get; set; }
    }
}