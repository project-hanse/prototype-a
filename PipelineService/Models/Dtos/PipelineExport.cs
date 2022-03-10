using System;

namespace PipelineService.Models.Dtos;

public class PipelineExport
{
	public Guid PipelineId { get; set; }
	public string PipelineName { get; set; }
	public string CreatedBy { get; set; }
	public DateTime CreatedOn { get; set; }
	public string OperationData { get; set; }
	public string PipelineData { get; set; }
}

public class Neo4JRelationShip<TS, TE>
{
	public int Id { get; set; }
	public string Label { get; set; }
	public string Type { get; set; }
	public Neo4JNode<TS> Start { get; set; }
	public Neo4JNode<TE> End { get; set; }
}

public class Neo4JNode<T>
{
	public string Type { get; set; }
	public int Id { get; set; }
	public string[] Labels { get; set; }
	public T Properties { get; set; }
}
