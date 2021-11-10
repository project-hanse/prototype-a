using System;
using System.Collections.Generic;

namespace PipelineService.Models.Dtos
{
    public class PipelineVisualizationDto
    {
        public Guid PipelineId { get; set; }
        public string PipelineName { get; set; }
        public ISet<VisNode> Nodes { get; } = new HashSet<VisNode>();
        public ISet<VisEdge> Edges { get; } = new HashSet<VisEdge>();
    }

    public class VisNode
    {
        public Guid? Id { get; set; }
        public string Label { get; set; }
    }

    public class VisEdge
    {
        public Guid? Id { get; set; }
        public Guid? From { get; set; }
        public Guid? To { get; set; }
    }
}
