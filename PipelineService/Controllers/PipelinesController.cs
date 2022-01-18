using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelineService.Extensions;
using PipelineService.Models.Dtos;
using PipelineService.Services;

namespace PipelineService.Controllers
{
	public class PipelinesController : BaseController
	{
		private readonly ILogger<PipelinesController> _logger;
		private readonly IPipelineExecutionService _pipelineExecutionService;
		private readonly IPipelinesDtoService _pipelinesDtoService;

		public PipelinesController(
			ILogger<PipelinesController> logger,
			IPipelineExecutionService pipelineExecutionService,
			IPipelinesDtoService pipelinesDtoService)
		{
			_logger = logger;
			_pipelineExecutionService = pipelineExecutionService;
			_pipelinesDtoService = pipelinesDtoService;
		}

		[HttpGet("create/defaults")]
		public async Task<int> GetDefaults()
		{
			return (await _pipelineExecutionService.CreateDefaultPipelines()).Count;
		}

		[HttpGet("templates")]
		public async Task<IList<PipelineInfoDto>> GetTemplates()
		{
			return await _pipelineExecutionService.GetTemplateInfoDtos();
		}

		[HttpPost("create/from/template")]
		public async Task<CreateFromTemplateResponse> CreatePipelineFromTemplate(CreateFromTemplateRequest request)
		{
			request.UserIdentifier = HttpContext.GetUsernameFromBasicAuthHeader();
			return await _pipelineExecutionService.CreatePipelineFromTemplate(request);
		}

		[HttpGet]
		public async Task<IActionResult> GetPipelineDtos()
		{
			return Ok(await _pipelineExecutionService.GetPipelineDtos(HttpContext.GetUsernameFromBasicAuthHeader()));
		}

		[HttpGet("{pipelineId:Guid}")]
		public async Task<IActionResult> GetPipelineDto(Guid pipelineId)
		{
			var pipelineDto = await _pipelineExecutionService.GetPipelineInfoDto(pipelineId);

			if (pipelineDto == null)
			{
				return NotFound($"No pipeline with id {pipelineId} exists");
			}

			return Ok(pipelineDto);
		}

		[HttpPost("{pipelineId:Guid}")]
		public async Task<IActionResult> UpdatePipeline(Guid pipelineId, [FromBody] PipelineInfoDto pipelineDto)
		{
			if (pipelineId != pipelineDto.Id)
			{
				return BadRequest("The pipeline id in the request body does not match the id in the url");
			}

			var updatedPipelineDto = await _pipelineExecutionService.UpdatePipeline(pipelineDto);

			if (updatedPipelineDto == null)
			{
				return NotFound($"No pipeline with id {pipelineId} exists");
			}

			return Ok(updatedPipelineDto);
		}

		[HttpGet("vis/{pipelineId:Guid}")]
		public async Task<IActionResult> GetPipelineForVisualization(Guid pipelineId)
		{
			var pipelineVisDto = await _pipelineExecutionService.GetPipelineForVisualization(pipelineId);
			if (pipelineVisDto == null)
			{
				return NotFound();
			}

			return Ok(pipelineVisDto);
		}

		[HttpGet("execute/{pipelineId:Guid}")]
		public async Task<IActionResult> ExecutePipeline(Guid pipelineId)
		{
			_logger.LogInformation("Executing pipeline {PipelineId}", pipelineId);

			var execution = await _pipelineExecutionService.ExecutePipeline(pipelineId);

			_logger.LogDebug("Execution of pipeline ({PipelineId}) with execution id {ExecutionId} started",
				pipelineId, execution);

			return Ok(execution);
		}

		[HttpGet("tuples")]
		public async Task<IActionResult> GetOperationTuples()
		{
			var tuples = (await _pipelinesDtoService.GetOperationTuples())
				.OrderBy(t => t.TupleDescription)
				.ToList();
			return Ok(tuples);
		}
	}
}
