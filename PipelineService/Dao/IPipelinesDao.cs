using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao
{
    public interface IPipelinesDao
    {
        /// <summary>
        /// Creates a default pipeline and stores it.
        /// </summary>
        /// <param name="id">The pipeline's id the new pipeline will be created withh</param>
        /// <returns>The default pipeline.</returns>
        public Task<Pipeline> Create(Guid id);

        /// <summary>
        /// Creates a bunch of default pipelines in the store.
        /// </summary>
        /// <param name="pipelines">The pipelines that will be stored if parameter is not null.</param>
        /// <returns>A list of newly created pipelines.</returns>
        Task<IList<Pipeline>> CreateDefaults(IList<Pipeline> pipelines = null);

        /// <summary>
        /// Adds a pipeline as is to the store.
        /// </summary>
        /// <param name="pipeline">The pipeline that will be stored</param>
        public Task Add(Pipeline pipeline);

        /// <summary>
        /// Loads a pipeline from the store by it's id.
        /// </summary>
        /// <param name="pipelineId">The pipeline's id</param>
        /// <exception cref="NotFoundException">If no pipeline with this id exists.</exception>
        /// <returns>The pipeline</returns>
        public Task<Pipeline> Get(Guid pipelineId);

        /// <summary>
        /// Loads a pipeline dto by it's id.
        /// </summary>
        /// <param name="pipelineId">The pipeline's id</param>
        /// <exception cref="NotFoundException">If no pipeline with this id exists.</exception>
        /// <returns>The pipeline's dto if the pipeline exists, otherwise null.</returns>
        Task<PipelineInfoDto> GetInfoDto(Guid pipelineId);

        /// <summary>
        /// Loads all available pipelines.
        /// </summary>
        /// <returns>A list of all pipelines</returns>
        public Task<IList<Pipeline>> Get();

        /// <summary>
        /// Loads dtos for all available pipelines
        /// </summary>
        /// <returns>A list of dtos of all pipelines.</returns>
        Task<IList<PipelineInfoDto>> GetDtos();

        /// <summary>
        /// Updates a pipeline in the store.
        /// </summary>
        /// <param name="pipeline">The pipeline that will be updated.</param>
        /// <returns>The updated pipeline</returns>
        Task<Pipeline> Update(Pipeline pipeline);
    }
}