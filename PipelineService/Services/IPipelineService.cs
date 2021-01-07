using System;
using System.Threading.Tasks;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services
{
    public interface IPipelineService
    {
        /// <summary>
        /// Creates a default pipeline and stores it.
        /// </summary>
        /// <returns>The default pipeline.</returns>
        public Task<Pipeline> CreateDefault();

        /// <summary>
        /// Loads a pipeline from the store by it's id.
        /// </summary>
        /// <param name="pipelineId">The pipeline's id</param>
        /// <returns>The pipeline</returns>
        public Task<Pipeline> GetPipeline(Guid pipelineId);
    }
}