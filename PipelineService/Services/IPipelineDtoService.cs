using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
    public interface IPipelineDtoService
    {
        public Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples();
        public Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples(Guid pipelineId);
        public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples();
        public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples(Guid pipelineId);
    }
}