using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao
{
    public interface IPipelineDao
    {
        /// <summary>
        /// Creates a default pipeline and stores it.
        /// </summary>
        /// <param name="id">The pipeline's id the new pipeline will be created withh</param>
        /// <returns>The default pipeline.</returns>
        public Task<Pipeline> Create(Guid id);

        /// <summary>
        /// Loads a pipeline from the store by it's id.
        /// </summary>
        /// <param name="pipelineId">The pipeline's id</param>
        /// <returns>The pipeline</returns>
        public Task<Pipeline> Get(Guid pipelineId);

        /// <summary>
        /// Loads all available pipelines.
        /// </summary>
        /// <returns>A list of all pipelines</returns>
        public Task<IList<Pipeline>> Get();
    }
}