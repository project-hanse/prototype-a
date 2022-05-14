using System;
using System.Collections.Generic;
using PipelineService.Models.Pipeline;

namespace PipelineService.Models.Dtos
{
	public class AddOperationRequest : BaseRequest
	{
		/// <summary>
		/// The pipeline that will be modified.
		/// </summary>
		public Guid PipelineId { get; set; }
		public IList<PredecessorOperationDto> PredecessorOperationDtos { get; set; } = new List<PredecessorOperationDto>();
		public OperationTemplate NewOperationTemplate { get; set; }
		public IDictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
	}

	public class PredecessorOperationDto
	{
		/// <summary>
		/// The operation's id who's output datasets will be the new input datasets for the new operation.
		/// </summary>
		public Guid? OperationId { get; set; }

		/// <summary>
		/// The operation's template id.
		/// </summary>
		public Guid? OperationTemplateId { get; set; }

		/// <summary>
		/// The output datasets of the operation that will be used as the input datasets for the new operation.
		/// </summary>
		public IList<Dataset> OutputDatasets { get; set; }
	}
}
