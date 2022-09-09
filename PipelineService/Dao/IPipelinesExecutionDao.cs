using System;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Enums;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Dao
{
	public interface IPipelinesExecutionDao
	{
		/// <summary>
		/// Creates a new <code>PipelineExecutionRecord</code> for a given pipeline.
		/// </summary>
		/// <param name="pipeline">The pipeline a new execution will be started for.</param>
		/// <param name="strategy">The strategy the execution plan will follow.</param>
		/// <returns>The execution record.</returns>
		Task<PipelineExecutionRecord> Create(Guid pipeline, ExecutionStrategy strategy = ExecutionStrategy.Lazy);

		/// <summary>
		/// Loads an execution by it's id or throws an exception if no execution for this id.
		/// </summary>
		/// <exception cref="NotFoundException">If no item with the execution id is found</exception>
		/// <param name="executionId">The execution's id</param>
		/// <param name="reload">Reloads the entities from the database; WARNING: very expensive since it hammers the database for each operation record if they are included.</param>
		/// <param name="includeOperationRecords">Indicates whether the nested operation execution records should be loaded.</param>
		/// <returns></returns>
		Task<PipelineExecutionRecord> Get(Guid executionId, bool includeOperationRecords = true, bool reload = false);

		/// <summary>
		/// Updates a given execution record in the store.
		/// </summary>
		/// <param name="execution">The execution record to be updated.</param>
		/// <returns></returns>
		Task<PipelineExecutionRecord> Update(PipelineExecutionRecord execution);

		/// <summary>
		/// Loads the last execution record available for a given pipeline.
		/// </summary>
		/// <param name="pipelineId"></param>
		/// <returns>The execution record if available; otherwise null</returns>
		Task<PipelineExecutionRecord> GetLastExecutionForPipeline(Guid pipelineId);

		/// <summary>
		/// Loads the execution record for an operation from the last completed operation of a pipeline.
		/// </summary>
		/// <param name="pipelineId"></param>
		/// <param name="operationId"></param>
		/// <returns></returns>
		Task<OperationExecutionRecord> GetLastCompletedExecutionForOperation(Guid pipelineId, Guid operationId);

		/// <summary>
		/// Stores a "hash at enqueuing" value for a given operation for a given execution.
		/// </summary>
		/// <param name="executionId"></param>
		/// <param name="operationId"></param>
		/// <param name="operationHash"></param>
		/// <param name="predecessorsHash"></param>
		Task StoreExecutionHash(Guid executionId, Guid operationId, string operationHash, string predecessorsHash);
	}
}
