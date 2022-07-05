using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Dtos;
using PipelineService.Models.MqttMessages;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

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
		/// Loads available pipeline templates as pipeline info dtos.
		/// </summary>
		/// <returns>A list of pipeline info dtos.</returns>
		Task<IList<PipelineInfoDto>> GetTemplateInfoDtos();

		/// <summary>
		/// Loads a pipeline by it's id.
		/// </summary>
		/// <param name="id">The pipeline's id.</param>
		/// <returns>The pipeline if it exists, otherwise <c>null</c>.</returns>
		Task<PipelineInfoDto> GetPipelineInfoDto(Guid id);

		/// <summary>
		/// Deletes a pipeline by it's id.
		/// </summary>
		/// <param name="pipelineId">The pipeline's id.</param>
		/// <returns>The info <c>PipelineInfoDto</c> of the deleted pipeline if the pipeline exists, otherwise <c>null</c>.</returns>
		Task<PipelineInfoDto> DeletePipeline(Guid pipelineId);

		/// <summary>
		/// Updates a pipeline.
		/// </summary>
		/// <param name="pipelineDto">The pipeline model used for the update.</param>
		/// <returns>The updated pipeline model</returns>
		Task<PipelineInfoDto> UpdatePipeline(PipelineInfoDto pipelineDto);

		/// <summary>
		/// Builds a DTO that represents a pipeline that can be used in a nis network for visualization of
		/// the pipeline graph.
		/// </summary>
		/// <param name="pipelineId">The pipelines id</param>
		/// <returns>If a pipeline with this id exists the corresponding DTO, otherwise null.</returns>
		Task<PipelineVisualizationDto> GetPipelineForVisualization(Guid pipelineId);

		/// <summary>
		/// Loads all pipelines stored for the current user.
		/// </summary>
		/// <returns>A list of pipelines</returns>
		Task<IList<PipelineInfoDto>> GetPipelineDtos(string userIdentifier);

		/// <summary>
		/// Starts the execution of a given pipeline.
		/// Provides an id that can be used to check the execution status.
		/// </summary>
		/// <param name="pipelineId">The pipeline's id</param>
		/// <exception cref="NotFoundException">If not pipeline with a given id can be found.</exception>
		/// <returns>The pipeline execution's id.</returns>
		public Task<Guid> ExecutePipeline(Guid pipelineId);

		/// <summary>
		/// Starts the execution of a pipeline and returns the execution record once the pipeline is fully executed.
		/// </summary>
		/// <remarks>
		/// This is a rather expensive operation and should be used with care.
		/// </remarks>
		/// <param name="pipelineId">The pipeline that will be executed.</param>
		/// <param name="skipIfExecuted">If this service (instance) knows about an previous execution of this pipeline the execution will be skipped and the previous record will be returned.</param>
		/// <param name="pollingDelay">The delay between checking of a pipeline is executed.</param>
		/// <returns></returns>
		public Task<PipelineExecutionRecord> ExecutePipelineSync(Guid pipelineId, bool skipIfExecuted = false, int pollingDelay = 1000);

		/// <summary>
		/// Handles the response of a worker after a node has been executed.
		/// </summary>
		/// <remarks>
		/// This covers both successful and unsuccessful execution.
		/// </remarks>
		/// <param name="response">The response message payload from the worker.</param>
		Task HandleExecutionResponse(OperationExecutedMessage response);

		/// <summary>
		/// Creates a new pipeline from a template for a user.
		/// </summary>
		/// <param name="request">The model containing information about the user and the pipeline.</param>
		/// <returns>The response with information about the new pipeline.</returns>
		Task<CreateFromTemplateResponse> CreatePipelineFromTemplate(CreateFromTemplateRequest request);
	}
}
