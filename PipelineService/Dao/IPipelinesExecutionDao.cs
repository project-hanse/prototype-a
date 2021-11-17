using System;
using System.Threading.Tasks;
using PipelineService.Exceptions;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Dao
{
	public interface IPipelinesExecutionDao
	{
		/// <summary>
		/// Creates a new <code>PipelineExecutionRecord</code> for a given pipeline.
		/// </summary>
		/// <param name="pipeline">The pipeline a new execution will be started for.</param>
		/// <returns>The execution record.</returns>
		Task<PipelineExecutionRecord> Create(Guid pipeline);

		/// <summary>
		/// Loads an execution by it's id or throws an exception if no execution for this id.
		/// </summary>
		/// <exception cref="NotFoundException">If no item with the execution id is found</exception>
		/// <param name="executionId">The execution's id</param>
		/// <returns></returns>
		Task<PipelineExecutionRecord> Get(Guid executionId);

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
	}
}
