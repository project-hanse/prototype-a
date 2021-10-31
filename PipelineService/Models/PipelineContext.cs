using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using Neo4jClient;
using Neo4jClient.DataAnnotations;
using Neo4jClient.DataAnnotations.Serialization;

namespace PipelineService.Models
{
	public class PipelineContext : AnnotationsContext
	{
		public virtual EntitySet<PipelinesRoot> PipelinesRoot { get; set; }
		public virtual EntitySet<Pipeline.Pipeline> Pipelines { get; set; }
		public virtual EntitySet<Pipeline.Node> Nodes { get; set; }


		public PipelineContext(IGraphClient graphClient) : base(graphClient)
		{
		}

		public PipelineContext(IGraphClient graphClient, EntityService entityService) : base(graphClient, entityService)
		{
		}

		public PipelineContext(IGraphClient graphClient, EntityResolver resolver) : base(graphClient, resolver)
		{
		}

		public PipelineContext(IGraphClient graphClient, EntityResolver resolver, EntityService entityService) : base(
			graphClient, resolver, entityService)
		{
		}

		public PipelineContext(IGraphClient graphClient, EntityConverter converter) : base(graphClient, converter)
		{
		}

		public PipelineContext(IGraphClient graphClient, EntityConverter converter, EntityService entityService) : base(
			graphClient, converter, entityService)
		{
		}

		protected PipelineContext(IGraphClient graphClient, EntityResolver resolver, EntityConverter converter,
			EntityService entityService) : base(graphClient, resolver, converter, entityService)
		{
		}
	}


	[Table(nameof(PipelinesRoot))]
	public class PipelinesRoot
	{
		public string Id { get; set; } = Identifier;

		public string Name { get; set; } = "PipelinesRoot";

		[Column("EXISTS")]
		public IEnumerable<Pipeline.Pipeline> Pipelines { get; set; }

		public static readonly string Identifier = "57079e85-b52f-459e-bd6d-64285be0f9a6";
	}
}
