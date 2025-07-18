using System;
using Microsoft.AspNetCore.Http;

namespace FileService.Models.Requests
{
	// same class exists in FilesService and PipelinesService
	// TODO: move to common library
	public class UploadFileRequest : BaseRequest
	{
		public IFormFile File { get; set; }
		public string FileName { get; set; }
		public DateTime? LastModified { get; set; }
		public string UserIdentifier { get; set; }
	}
}
