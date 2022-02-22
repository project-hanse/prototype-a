using System.Collections.Generic;
using System.Net;

namespace PipelineService.Models.Dtos
{
	public class BaseResponse
	{
		public bool Success { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public IList<Error> Errors { get; } = new List<Error>();
	}

	public class Error
	{
		public string Message { get; set; }
		public string Code { get; set; }
	}
}
