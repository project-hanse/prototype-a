using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Dtos;
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
		Task<PipelineInfoDto> GetPipelineInfoDto(Guid id);

		/// <summary>
		/// Builds a DTO that represents a pipeline that can be used in a nis network for visualization of
		/// the pipeline graph.
		/// </summary>
		/// <param name="pipelineId">The pipelines id</param>
		/// <returns>If a pipeline with this id exists the corresponding DTO, otherwise null.</returns>
		Task<PipelineVisualizationDto> GetPipelineForVisualization(Guid pipelineId);

		/// <summary>
		/// Loads all pipelines stored in this instance (this will be changed ofe pipelines are stored in a dedicated
		/// microservice). 
		/// </summary>
		/// <returns>A list of pipelines</returns>
		Task<IList<PipelineInfoDto>> GetPipelineDtos();

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
