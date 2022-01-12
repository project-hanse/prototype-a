using System.Threading.Tasks;
using FileService.Extensions;
using FileService.Models.Requests;
using FileService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers
{
	public class FilesController : BaseController
	{
		private readonly S3FilesService _filesService;

		public FilesController(S3FilesService filesService)
		{
			_filesService = filesService;
		}

		[HttpGet("info")]
		public async Task<IActionResult> GetInfosForUser()
		{
			return Ok(await _filesService.GetFileInfosForUser(HttpContext.GetUsernameFromBasicAuthHeader()));
		}

		[HttpPost("upload")]
		public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
		{
			request.UserIdentifier = HttpContext.GetUsernameFromBasicAuthHeader();

			if (request.File != null)
			{
				return Ok(await _filesService.UploadFile(request));
			}

			return BadRequest();
		}

		[HttpGet("plot")]
		public async Task<IActionResult> GetPlot([FromQuery] string store, [FromQuery] string key)
		{
			var stream = await _filesService.GetFile(store, key);
			if (stream != null)
			{
				return new FileStreamResult(stream, "image/svg+xml");
			}

			return NotFound();
		}
	}
}
