using System;

namespace FileService.Models.Dtos
{
	public class FileInfoDto
	{
		public string FileName { get; set; }
		public DateTime LastModified { get; set; }
		public long? Size { get; set; }
	}
}
