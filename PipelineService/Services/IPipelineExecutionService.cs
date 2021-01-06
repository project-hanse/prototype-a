using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services
{
    public interface IPipelineExecutionService
    {
        /// <summary>
        /// Starts the execution of a given pipeline.
        /// Provides an id that can be used to check the execution status.
        /// </summary>
        /// <param name="pipelineId">The pipeline's id</param>
        /// <returns>The pipeline execution's id. Returns null if no pipeline with the given id was found.</returns>
        public Task<Guid?> ExecutePipeline(Guid pipelineId);

        /// <summary>
        /// Computes a string representation of the execution status for an execution id.
        /// </summary>
        /// <param name="executionId">The execution's id</param>
        /// <returns>The status as a string (RUNNING, ABORTED, COMPLETE)</returns>
        public Task<string> GetExecutionStatus(Guid executionId);

        /// <summary>
        /// Creates a new <code>PipelineExecutionRecord</code> for a given pipeline and enqueues the execution of the
        /// first blocks of the given pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline a new execution will be started for.</param>
        /// <returns>The execution's id.</returns>
        Task<Guid> CreateExecution(Pipeline pipeline);

        /// <summary>
        /// Selects the next blocks to be executed for a given execution of a pipeline.
        /// Might return empty list if no more blocks need to be executed.
        /// </summary>
        /// <exception cref="InvalidIdException">If the execution does not match the pipeline.</exception>
        /// /// <exception cref="NotFoundException">If the execution does not match the pipeline.</exception>
        /// <param name="executionId">The execution's id.</param>
        /// <param name="pipeline">The pipeline that is being executed.</param>
        /// <returns>A list of blocks that need to be executed next inorder to complete the execution of the pipeline</returns>
        Task<IList<Block>> SelectNextBlocks(Guid executionId, Pipeline pipeline);
    }
}