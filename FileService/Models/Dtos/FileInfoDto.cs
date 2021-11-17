using System;

namespace FileService.Models.Dtos
{
	public class FileInfoDto
	{
		public string FileName { get; set; }
		public string FileExtension { get; set; }
		public DateTime LastModified { get; set; }
		public long? Size { get; set; }
		public string BucketName { get; set; }
		public string ObjectKey { get; set; }
	}
}
