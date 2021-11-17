using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public abstract class BaseController : ControllerBase
	{
	}
}
