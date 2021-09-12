using System;
using PipelineService.Models.Enums;

namespace PipelineService.Models.Dtos
{
    public class OperationDto
    {
        public Guid OperationId { get; set; }
        public string OperationName { get; set; }
        public string OperationFullName { get; set; }
        public string Framework { get; set; }
        public OperationInputTypes OperationInputType { get; set; }
    }
}