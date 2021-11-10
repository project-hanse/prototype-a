using System;

namespace PipelineService.Models.Dtos
{
    public class PipelineInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}