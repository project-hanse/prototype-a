using System;
using System.Threading.Tasks;

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
    }
}