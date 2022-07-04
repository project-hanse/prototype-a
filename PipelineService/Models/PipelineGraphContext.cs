using Neo4jClient;
using Neo4jClient.DataAnnotations;
using Neo4jClient.DataAnnotations.Serialization;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models
{
	public class PipelineGraphContext : AnnotationsContext
	{
		public virtual EntitySet<Pipeline.Pipeline> Pipelines { get; set; }
		public virtual EntitySet<Operation> Operations { get; set; }

		public PipelineGraphContext(IGraphClient graphClient) : base(graphClient)
		{
		}

		public PipelineGraphContext(IGraphClient graphClient, EntityService entityService) : base(graphClient, entityService)
		{
		}

		public PipelineGraphContext(IGraphClient graphClient, EntityResolver resolver) : base(graphClient, resolver)
		{
		}

		public PipelineGraphContext(IGraphClient graphClient, EntityResolver resolver, EntityService entityService) : base(
			graphClient, resolver, entityService)
		{
		}

		public PipelineGraphContext(IGraphClient graphClient, EntityConverter converter) : base(graphClient, converter)
		{
		}

		public PipelineGraphContext(IGraphClient graphClient, EntityConverter converter, EntityService entityService) : base(
			graphClient, converter, entityService)
		{
		}

		protected PipelineGraphContext(IGraphClient graphClient, EntityResolver resolver, EntityConverter converter,
			EntityService entityService) : base(graphClient, resolver, converter, entityService)
		{
		}
	}
}
