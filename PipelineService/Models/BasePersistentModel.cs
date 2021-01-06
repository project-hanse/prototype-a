using System;

namespace PipelineService.Models
{
    public abstract record BasePersistentModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}