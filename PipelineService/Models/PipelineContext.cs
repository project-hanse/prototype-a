using Neo4jClient;
using Neo4jClient.DataAnnotations;
using Neo4jClient.DataAnnotations.Serialization;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models
{
	public class PipelineContext : AnnotationsContext
	{
		public virtual EntitySet<Pipeline.Pipeline> Pipelines { get; set; }
		public virtual EntitySet<Node> Nodes { get; set; }
		public virtual EntitySet<NodeSingleInput> NodesSingleInput { get; set; }
		public virtual EntitySet<NodeDoubleInput> NodesDoubleInput { get; set; }
		public virtual EntitySet<NodeFileInput> NodesFileInput { get; set; }

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
}
