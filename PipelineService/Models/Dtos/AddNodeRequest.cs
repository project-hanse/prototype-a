using System;
using System.Collections.Generic;

namespace PipelineService.Models.Dtos
{
	public class AddNodeRequest : BaseRequest
	{
		public Guid PipelineId { get; set; }
		public IList<Guid> PredecessorNodeIds { get; set; }
		public OperationDto Operation { get; set; }
	}
}
