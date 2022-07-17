using System;

namespace PipelineService.Models.Dtos
{
	public class CreateFromTemplateRequest : BaseRequest
	{
		public Guid? TemplateId { get; set; }
	}
}
