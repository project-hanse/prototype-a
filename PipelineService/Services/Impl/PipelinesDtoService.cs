using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Dao;
using PipelineService.Models.Dtos;

namespace PipelineService.Services.Impl
{
	public class PipelinesDtoService : IPipelinesDtoService
	{
		private readonly IPipelinesDao _pipelinesesDao;
		private readonly IPipelinesExecutionDao _pipelinesExecutionDao;

		public PipelinesDtoService(IPipelinesDao pipelinesesDao, IPipelinesExecutionDao pipelinesExecutionDao)
		{
			_pipelinesesDao = pipelinesesDao;
			_pipelinesExecutionDao = pipelinesExecutionDao;
		}

		public async Task<IList<OperationTuples>> GetOperationTuples()
		{
			// TODO: check if pipeline has been successfully executed
			return await _pipelinesesDao.GetOperationTuples();
		}
	}
}
