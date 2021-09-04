using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Dao;
using PipelineService.Models.Dtos;

namespace PipelineService.Services.Impl
{
    public class PipelineDtoService : IPipelineDtoService
    {
        private IPipelineDao _pipelineDao;

        public PipelineDtoService(IPipelineDao pipelineDao)
        {
            _pipelineDao = pipelineDao;
        }

        public Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples()
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples()
        {
            throw new System.NotImplementedException();
        }
    }
}