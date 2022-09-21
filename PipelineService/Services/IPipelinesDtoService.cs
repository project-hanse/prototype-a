using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
	public interface IPipelinesDtoService
	{
		public Task<IList<OperationTuples>> GetOperationTuples();
		public Task<PipelineExport> ExportPipeline(Guid pipelineId);

		/// <summary>
		/// Parses the data in the <c>exportObject</c> and creates a new pipeline.
		/// </summary>
		/// <param name="exportObject"></param>
		/// <exception cref="InvalidDataException">If the provided data in the export object can not be parsed.</exception>
		/// <returns></returns>
		public Task<Guid> ImportPipeline(PipelineExport exportObject);

		public Task<Guid> ImportPipelineCandidate(PipelineCandidate pipelineCandidate, string username = null);

		/// <summary>
		/// Automatically selects pipeline candidates that should be processed next and schedules their execution.
		/// </summary>
		/// <param name="includeIncomplete">Set to true if incomplete candidate executions should be rescheduled.</param>
		/// <returns>The number of candidates scheduled.</returns>
		Task<int> AutoSchedulePipelineCandidates(bool includeIncomplete = true);

		/// <summary>
		/// Schedules all pipeline candidates that have not been completed yet.
		/// </summary>
		/// <returns>The number of candidates scheduled.</returns>
		Task<int> ScheduleIncompleteCandidatesProcessing();

		/// <summary>
		/// Schedules the processing of a set of pipeline candidates.
		/// </summary>
		/// <param name="candidateIds">The pipeline candidates that should be schedules for processing.</param>
		/// <returns>The number of candidates scheduled.</returns>
		Task<int> SchedulePipelineCandidatesProcessing(IList<Guid> candidateIds);

		/// <summary>
		/// Processes a pipeline candidate.
		/// "Processing as a candidate" means that the pipeline is executed. If the execution fails, the pipeline's
		/// operation configurations will be randomized and the pipeline will be executed again.
		/// </summary>
		/// <remarks>
		/// WARNING: This method executes the pipeline synchronously - it will block the current thread until the executions are complete.
		/// </remarks>
		/// <param name="metricId">The metric record processing metrics will be recorded in.</param>
		/// <param name="pipelineId">The pipeline that should be processed as a candidate.</param>
		/// <returns></returns>
		Task ProcessPipelineAsCandidate(Guid metricId, Guid pipelineId);
	}
}
