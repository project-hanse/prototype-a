using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PipelineService.Services;

namespace PipelineService.Controllers
{
    public class OperationsController : BaseController
    {
        private readonly IOperationsService _operationsService;

        public OperationsController(IOperationsService operationsService)
        {
            _operationsService = operationsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOperations()
        {
            return Ok(await _operationsService.GetOperationDtos());
        }
    }
}