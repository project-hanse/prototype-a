using System;
using System.Collections.Generic;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.Dtos
{
	public class PipelineVisualizationDto
	{
		public Guid PipelineId { get; set; }
		public string PipelineName { get; set; }
		public ISet<VisualizationOperationDto> Nodes { get; } = new HashSet<VisualizationOperationDto>();
		public ISet<VisEdge> Edges { get; } = new HashSet<VisEdge>();
	}

	public class VisNode
	{
		public Guid? Id { get; set; }
		public string Label { get; set; }
	}

	public class VisualizationOperationDto : VisNode
	{
		public IList<Dataset> Inputs { get; set; }
		public Dataset Output { get; set; }
		public string OperationIdentifier { get; set; }
	}

	public class VisEdge
	{
		public Guid? Id { get; set; }
		public Guid? From { get; set; }
		public Guid? To { get; set; }
	}
}
