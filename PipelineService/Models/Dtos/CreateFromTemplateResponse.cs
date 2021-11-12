using System;

namespace PipelineService.Models.Dtos
{
	public class CreateFromTemplateResponse : BaseResponse
	{
		public Guid PipelineId { get; set; }
	}
}
