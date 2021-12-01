using System;
using System.Collections.Generic;

namespace PipelineService.Models.Dtos
{
	public class RemoveOperationsRequest : BaseRequest
	{
		public Guid PipelineId { get; set; }
		public IList<Guid> OperationIdsToBeRemoved { get; set; }
	}
}
