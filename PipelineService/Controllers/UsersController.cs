using Microsoft.AspNetCore.Mvc;
using PipelineService.Extensions;

namespace PipelineService.Controllers
{
	public class UsersController : BaseController
	{
		[HttpGet("current/info")]
		public IActionResult GetUserInfo()
		{
			var username = HttpContext.GetUsernameFromBasicAuthHeader();

			if (username != null)
			{
				return Ok(new { username });
			}

			return NotFound();
		}
	}
}
