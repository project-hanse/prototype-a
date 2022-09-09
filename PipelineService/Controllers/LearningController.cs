using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelineService.Services;

namespace PipelineService.Controllers;

public class LearningController : BaseController
{
	private readonly ILogger<LearningController> _logger;

	public LearningController(ILogger<LearningController> logger)
	{
		_logger = logger;
	}

	[HttpGet("train/all/background")]
	public IActionResult TrainAllBackground()
	{
		BackgroundJob.Enqueue<ILearningServiceClient>(l => l.TrainModels());
		_logger.LogInformation("Triggered training of all models in background");
		return Ok();
	}
}
