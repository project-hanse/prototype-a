using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

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
        /// Enqueues a block to be executed by the appropriate worker. 
        /// </summary>
        /// <param name="executionId">The execution this block belongs to.</param>
        /// <param name="block">The block to be executed.</param>
        public Task EnqueueBlock(Guid executionId, Block block);

        /// <summary>
        /// Loads an execution by it's id or throws an exception if no execution for this id.
        /// </summary>
        /// <exception cref="NotFoundException">If no item with the execution id is found</exception>
        /// <param name="executionId">The execution's id</param>
        /// <returns></returns>
        Task<PipelineExecutionRecord> GetById(Guid executionId);

        /// <summary>
        /// Creates a new <code>PipelineExecutionRecord</code> for a given pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline a new execution will be started for.</param>
        /// <returns>The execution record.</returns>
        Task<PipelineExecutionRecord> CreateExecution(Pipeline pipeline);

        /// <summary>
        /// Selects the next blocks to be executed for a given execution of a pipeline.
        /// Might return empty list if no blocks need to be executed at the moment.
        /// </summary>
        /// 
        /// <exception cref="InvalidOperationException">If no execution for a given execution id exists.</exception>
        /// <exception cref="ArgumentException">If the execution does not match the pipeline.</exception>
        /// <param name="executionId">The execution's id.</param>
        /// <param name="pipeline">The pipeline that is being executed.</param>
        /// <returns>A list of blocks that need to be executed next inorder to complete the execution of the pipeline</returns>
        Task<IList<Block>> SelectNextBlocks(Guid executionId, Pipeline pipeline);

        /// <summary>
        /// Marks a block as executed in an execution.
        /// </summary>
        /// <param name="executionId">The execution's id a block has been executed in.</param>
        /// <param name="blockId">The block that will be moved from status in execution to executed.</param>
        /// <returns>True if there are still block in status in_execution.</returns>
        Task<bool> MarkBlockAsExecuted(Guid executionId, Guid blockId);
    }
}