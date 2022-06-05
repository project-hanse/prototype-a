using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PipelineService.Models.Dtos;
using PipelineService.Services;

namespace PipelineService.Controllers
{
	public class OperationTemplatesController : BaseController
	{
		private readonly IOperationTemplatesService _operationTemplatesService;

		public OperationTemplatesController(IOperationTemplatesService operationTemplatesService)
		{
			_operationTemplatesService = operationTemplatesService;
		}

		[HttpGet]
		public async Task<IActionResult> GetOperations([FromQuery] GetOperationTemplatesRequest request)
		{
			return Ok(await _operationTemplatesService.GetOperationDtos(request));
		}

		[HttpGet("grouped")]
		public async Task<IActionResult> GetOperationsGrouped([FromQuery] GetOperationTemplatesRequest request)
		{
			return Ok((await _operationTemplatesService.GetOperationDtos(request))
				.GroupBy(op => op.SectionTitle, (key, group) => new { SectionTitle = key, Operations = group.ToList() }));
		}
	}
}
