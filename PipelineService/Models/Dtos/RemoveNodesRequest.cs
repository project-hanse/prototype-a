using System;
using System.Collections.Generic;

namespace PipelineService.Models.Dtos
{
	public class RemoveNodesRequest : BaseRequest
	{
		public Guid PipelineId { get; set; }
		public IList<Guid> NodeIdsToBeRemoved { get; set; }
	}
}
