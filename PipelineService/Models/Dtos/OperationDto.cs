using System;
using System.Collections.Generic;
using PipelineService.Models.Enums;

namespace PipelineService.Models.Dtos
{
    public class OperationDto
    {
        public Guid OperationId { get; set; }
        public string OperationName { get; set; }
        public string OperationFullName { get; set; }
        public OperationInputTypes OperationInputType { get; set; }
        public string Framework { get; set; }
        public string FrameworkVersion { get; set; }
        public string SectionTitle { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Signature { get; set; }
        public string SignatureName { get; set; }
        public string Returns { get; set; }
        public string ReturnsDescription { get; set; }
        public string SourceUrl { get; set; }
        public Dictionary<string, string> DefaultConfig { get; set; }
    }
}