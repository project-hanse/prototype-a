using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.MqttMessages;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services
{
    public interface IPipelineExecutionService
    {
        /// <summary>
        /// Creates a default pipeline.
        /// </summary>
        /// <returns>The new pipeline.</returns>
        Task<IList<Pipeline>> CreateDefaultPipelines();

        /// <summary>
        /// Loads a pipeline by it's id.
        /// </summary>
        /// <param name="id">The pipeline's id.</param>
        /// <exception cref="NotFoundException">If not pipeline with a given id can be found.</exception>
        /// <returns>The pipeline</returns>
        Task<Pipeline> GetPipeline(Guid id);

        /// <summary>
        /// Loads all pipelines stored in this instance (this will be changed ofe pipelines are stored in a dedicated
        /// microservice). 
        /// </summary>
        /// <returns>A list of pipelines</returns>
        Task<IList<Pipeline>> GetPipelines();

        /// <summary>
        /// Starts the execution of a given pipeline.
        /// Provides an id that can be used to check the execution status.
        /// </summary>
        /// <param name="pipelineId">The pipeline's id</param>
        /// <exception cref="NotFoundException">If not pipeline with a given id can be found.</exception>
        /// <returns>The pipeline execution's id.</returns>
        public Task<Guid> ExecutePipeline(Guid pipelineId);

        /// <summary>
        /// Handles the response of a worker after a node has been executed.
        /// </summary>
        /// <remarks>
        /// This covers both successful and unsuccessful execution.
        /// </remarks>
        /// <param name="response">The response message payload from the worker.</param>
        Task HandleExecutionResponse(NodeExecutionResponse response);
    }
}