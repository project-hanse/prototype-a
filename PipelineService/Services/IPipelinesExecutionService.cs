using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;
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
		Task<PaginatedList<PipelineInfoDto>> GetPipelineDtos(Pagination pagination, string userIdentifier);

		/// <summary>
		/// Checks is a pipeline with a given id has already been executed.
		/// </summary>
		Task<bool> HasBeenExecuted(Guid pipelineId);

		/// <summary>
		/// Starts the execution of a given pipeline.
		/// Provides an id that can be used to check the execution status.
		/// </summary>
		/// <param name="pipelineId">The pipeline's id</param>
		/// <param name="skipIfExecuted">If this service (instance) knows about an previous execution of this pipeline the execution will be skipped and the previous record will be returned.</param>
		/// <param name="strategy">The pipeline execution strategy (<c>ExecutionStrategy.Lazy</c> or <c>ExecutionStrategy.Eager</c>)</param>
		/// <param name="allowResultCaching"></param>
		/// <exception cref="NotFoundException">If not pipeline with a given id can be found.</exception>
		/// <returns>The pipeline execution's id.</returns>
		public Task<Guid> ExecutePipeline(Guid pipelineId, bool skipIfExecuted = false,
			ExecutionStrategy strategy = ExecutionStrategy.Lazy, bool allowResultCaching = true);

		/// <summary>
		/// Starts the execution of a pipeline and returns the execution record once the pipeline is fully executed.
		/// </summary>
		/// <remarks>
		/// This is a rather expensive operation and should be used with care.
		/// Check if <c>ExecutePipeline(...)</c> with the callback function is sufficient.
		/// </remarks>
		/// <param name="pipelineId">The pipeline that will be executed.</param>
		/// <param name="skipIfExecuted">If this service (instance) knows about an previous execution of this pipeline the execution will be skipped and the previous record will be returned.</param>
		/// <returns></returns>
		public Task<PipelineExecutionRecord> ExecutePipelineSync(Guid pipelineId, bool skipIfExecuted = false);

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

		Task<IList<object>> GetTopologicalSort(Guid pipelineId, ExecutionStrategy strategy);
	}
}
