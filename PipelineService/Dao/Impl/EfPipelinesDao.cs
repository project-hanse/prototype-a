using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Context;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao.Impl
{
	public class EfPipelinesDao : IPipelinesDao
	{
		private readonly ILogger<EfPipelinesDao> _logger;
		private readonly PipelineContext _context;

		public EfPipelinesDao(ILogger<EfPipelinesDao> logger, PipelineContext context)
		{
			_logger = logger;
			_context = context;
		}

		public Task<Pipeline> Create(Guid id)
		{
			throw new NotImplementedException();
		}

		public async Task<IList<Pipeline>> CreateDefaults(IList<Pipeline> pipelines = null)
		{
			var newDefaultPipelines = pipelines ?? HardcodedDefaultPipelines.NewDefaultPipelines();
			await _context.AddRangeAsync(newDefaultPipelines);
			await _context.SaveChangesAsync();
			return newDefaultPipelines;
		}

		public Task Add(Pipeline pipeline)
		{
			throw new NotImplementedException();
		}

		public Task<Pipeline> Get(Guid pipelineId)
		{
			throw new NotImplementedException();
		}

		public Task<PipelineInfoDto> GetInfoDto(Guid pipelineId)
		{
			throw new NotImplementedException();
		}

		public Task<IList<Pipeline>> Get()
		{
			throw new NotImplementedException();
		}

		public Task<IList<PipelineInfoDto>> GetDtos()
		{
			throw new NotImplementedException();
		}

		public Task<Pipeline> Update(Pipeline pipeline)
		{
			throw new NotImplementedException();
		}
	}
}
