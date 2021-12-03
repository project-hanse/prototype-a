using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> GetOperations()
		{
			return Ok(await _operationTemplatesService.GetOperationDtos());
		}

		[HttpGet("grouped")]
		public async Task<IActionResult> GetOperationsGrouped()
		{
			return Ok((await _operationTemplatesService.GetOperationDtos())
				.GroupBy(op => op.SectionTitle, (key, group) => new { SectionTitle = key, Operations = group.ToList() }));
		}
	}
}
