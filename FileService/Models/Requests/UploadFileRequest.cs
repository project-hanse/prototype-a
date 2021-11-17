using System;
using Microsoft.AspNetCore.Http;

namespace FileService.Models.Requests
{
	public class UploadFileRequest : BaseRequest
	{
		public IFormFile File { get; set; }
		public string FileName { get; set; }
		public DateTime? LastModified { get; set; }
		public string UserIdentifier { get; set; }
	}
}
