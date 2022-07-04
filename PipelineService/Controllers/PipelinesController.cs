using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
		private readonly IPipelineCandidateService _pipelineCandidateService;

		public PipelinesController(
			ILogger<PipelinesController> logger,
			IPipelineExecutionService pipelineExecutionService,
			IPipelinesDtoService pipelinesDtoService,
			IPipelineCandidateService pipelineCandidateService)
		{
			_logger = logger;
			_pipelineExecutionService = pipelineExecutionService;
			_pipelinesDtoService = pipelinesDtoService;
			_pipelineCandidateService = pipelineCandidateService;
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

		[HttpDelete("{pipelineId:Guid}")]
		public async Task<IActionResult> DeletePipeline(Guid pipelineId)
		{
			var pipelineDto = await _pipelineExecutionService.DeletePipeline(pipelineId);

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

		[HttpGet("export/{pipelineId:Guid}")]
		public async Task ExportPipeline(Guid pipelineId)
		{
			var ret = await _pipelinesDtoService.ExportPipeline(pipelineId);
			if (ret == null)
			{
				Response.StatusCode = 404;
				return;
			}

			ret.CreatedBy = HttpContext.GetUsernameFromBasicAuthHeader();

			var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ret));
			await using var stream = new MemoryStream(bytes);
			Response.Headers.Add("Content-Disposition", $"attachment; pipeline-export-{pipelineId}.json");
			Response.Headers.Add("Content-Length", stream.Length.ToString());
			Response.ContentType = "plain/text";
			stream.WriteTo(Response.BodyWriter.AsStream());
		}

		[HttpGet("candidate")]
		public async Task<PaginatedList<PipelineCandidate>> GetPipelineCandidates([FromQuery] Pagination pagination)
		{
			return await _pipelineCandidateService.GetPipelineCandidateDtos(pagination);
		}

		[HttpGet("candidate/import/{pipelineCandidateId:Guid}")]
		public async Task<IActionResult> ImportPipelineCandidate(Guid pipelineCandidateId,
			[FromQuery]
			bool deleteAfterImport = false,
			[FromQuery]
			string username = null)
		{
			var candidate = await _pipelineCandidateService.GetCandidateById(pipelineCandidateId);
			var pipelineId = await _pipelinesDtoService.ImportPipelineCandidate(candidate, username);
			if (deleteAfterImport)
			{
				await _pipelineCandidateService.DeletePipelineCandidate(pipelineCandidateId);
			}

			return Ok(pipelineId);
		}

		[HttpGet("candidate/process")]
		public async Task<IActionResult> ProcessPipelineCandidate([FromQuery] int numberOfCandidates = 1)
		{
			await _pipelinesDtoService.ProcessPipelineCandidates(numberOfCandidates);
			return Ok(numberOfCandidates);
		}


		[HttpPost("import")]
		public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
		{
			// TODO: this could be moved to middleware
			request.UserIdentifier = HttpContext.GetUsernameFromBasicAuthHeader();

			if (request.File == null) return BadRequest();

			var json = await new StreamReader(request.File.OpenReadStream()).ReadToEndAsync();

			if (string.IsNullOrEmpty(json))
				return BadRequest(new BaseResponse
				{
					Errors =
					{
						new Error
						{
							Code = "F400",
							Message = "No data was found in the file"
						}
					}
				});

			var exportObject = JsonConvert.DeserializeObject<PipelineExport>(json);
			if (exportObject == null)
				return BadRequest(new BaseResponse
				{
					Errors =
					{
						new Error
						{
							Code = "F400",
							Message = "Failed to deserialize content"
						}
					}
				});
			exportObject.CreatedBy = request.UserIdentifier;

			var pipelineId = await _pipelinesDtoService.ImportPipeline(exportObject);
			return Ok(new ImportPipelineResponse
			{
				PipelineId = pipelineId
			});
		}
	}
}
