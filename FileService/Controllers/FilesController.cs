using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers
{
	public class FilesController : BaseController
	{
		[HttpPost("upload")]
		public async Task<IActionResult> Upload([FromForm] FileForm multipartForm)
		{
			var folderName = Path.Combine("Resources", "Images");
			var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

			if (multipartForm?.File != null)
			{
				var fullPath = Path.Combine(pathToSave, multipartForm.File.FileName);

				using (var stream = new FileStream(fullPath, FileMode.Create))
				{
					// TODO store in S3 bucket
					// 	await multipartForm.File.CopyToAsync(stream);
				}

				return Ok();
			}

			return BadRequest();
		}

		public class FileForm
		{
			public DateTime? LastModified { get; set; }
			public IFormFile File { get; set; }
		}
	}
}
