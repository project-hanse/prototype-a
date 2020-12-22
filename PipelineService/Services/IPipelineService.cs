using System;
using System.Threading.Tasks;
using PipelineService.Models;

namespace PipelineService.Services
{
    public interface IPipelineService
    {
        public Task<Pipeline> GetPipeline(Guid pipelineId);

        public Task<Pipeline> ExecutePipeline(Guid pipelineId);
    }
}