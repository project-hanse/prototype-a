using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelineService.Models.Dtos;
using PipelineService.Services;

namespace PipelineService.Controllers
{
	public class OperationsController : BaseController
	{
		private readonly ILogger<OperationsController> _logger;
		private readonly IOperationsService _operationsService;
		private readonly IPipelineExecutionService _pipelineExecutionService;

		public OperationsController(
			ILogger<OperationsController> logger,
			IOperationsService operationsService,
			IPipelineExecutionService pipelineExecutionService)
		{
			_logger = logger;
			_operationsService = operationsService;
			_pipelineExecutionService = pipelineExecutionService;
		}

		[HttpGet("{pipelineId:Guid}/{nodeId:Guid}/datasets/input")]
		public async Task<IActionResult> GetInputDatasetIds(Guid pipelineId, Guid nodeId)
		{
			_logger.LogInformation("Loading input datasets for {OperationId} of pipeline {PipelineId}",
				nodeId, pipelineId);

			var result = await _operationsService.GetInputDatasetKeysForOperation(pipelineId, nodeId);

			if (result == null)
			{
				return NotFound();
			}

			return Ok(result);
		}

		[HttpGet("{pipelineId:Guid}/{operationId:Guid}/output/dataset")]
		public async Task<IActionResult> GetResultHash(Guid pipelineId, Guid operationId)
		{
			var dataset = await _operationsService.GetOutputDataset(pipelineId, operationId);
			if (dataset == null)
			{
				return NotFound();
			}

			return Ok(dataset);
		}

		[HttpPost("add")]
		public async Task<IActionResult> AddOperation(AddOperationRequest request)
		{
			var response = await _operationsService.AddOperationToPipeline(request);

			if (!response.Success) return BadRequest(response);

			response.PipelineVisualizationDto =
				await _pipelineExecutionService.GetPipelineForVisualization(response.PipelineId);
			return Ok(response);
		}

		[HttpPost("remove")]
		public async Task<IActionResult> RemoveOperations(RemoveOperationsRequest request)
		{
			var response = await _operationsService.RemoveOperationsFromPipeline(request);

			if (!response.Success) return BadRequest(response);

			response.PipelineVisualizationDto =
				await _pipelineExecutionService.GetPipelineForVisualization(response.PipelineId);
			return Ok(response);
		}

		[HttpGet("{pipelineId:Guid}/{operationId:Guid}/config")]
		public async Task<IActionResult> GetConfiguration(Guid pipelineId, Guid operationId)
		{
			var config = await _operationsService.GetConfig(pipelineId, operationId);
			if (config == null)
			{
				return NotFound();
			}

			return Ok(config);
		}

		[HttpPost("{pipelineId:Guid}/{operationId:Guid}/config")]
		public async Task<IActionResult> UpdateConfiguration(
			Guid pipelineId, Guid operationId, Dictionary<string, string> config)
		{
			var success = await _operationsService.UpdateConfig(pipelineId, operationId, config);
			return success ? Ok() : BadRequest();
		}
	}
}
