using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Models.Dtos;

namespace PipelineService.Services.Impl
{
	public class PipelinesDtoService : IPipelinesDtoService
	{
		private readonly ILogger<PipelinesDtoService> _logger;
		private readonly IPipelinesDao _pipelinesDao;
		private readonly IPipelinesExecutionDao _pipelinesExecutionDao;

		public PipelinesDtoService(ILogger<PipelinesDtoService> logger,
			IPipelinesDao pipelinesDao,
			IPipelinesExecutionDao pipelinesExecutionDao)
		{
			_logger = logger;
			_pipelinesDao = pipelinesDao;
			_pipelinesExecutionDao = pipelinesExecutionDao;
		}

		public async Task<IList<OperationTuples>> GetOperationTuples()
		{
			// TODO: check if pipeline has been successfully executed
			return await _pipelinesDao.GetOperationTuples();
		}

		public async Task<PipelineExport> ExportPipeline(Guid pipelineId)
		{
			_logger.LogDebug("Exporting pipeline {PipelineId}", pipelineId);
			var p = await _pipelinesDao.GetInfoDto(pipelineId);
			if (p == null)
			{
				_logger.LogInformation("Pipeline {PipelineId} not found", pipelineId);
				return null;
			}

			return await _pipelinesDao.ExportPipeline(pipelineId);
		}
	}
}
