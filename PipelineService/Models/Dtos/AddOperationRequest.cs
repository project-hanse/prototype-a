using System;
using System.Collections.Generic;

namespace PipelineService.Models.Dtos
{
	public class AddOperationRequest : BaseRequest
	{
		public Guid PipelineId { get; set; }
		public IList<Guid> PredecessorOperationIds { get; set; }
		public OperationTemplate OperationTemplate { get; set; }
		public IDictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
	}
}
