using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao
{
	public interface IPipelinesDao
	{
		/// <summary>
		/// Sets up indices for the pipelines database.
		/// </summary>
		Task Setup();

		/// <summary>
		/// Creates a pipeline with all its nodes in the database.
		/// </summary>
		/// <remarks><b>Warning:</b> The current implementation is very ineffective and takes quiet some time.</remarks>
		/// <param name="pipelines">The pipeline records that will be persistently stored.</param>
		/// <returns>A list of all pipelines that have been created.</returns>
		Task<IList<Pipeline>> CreatePipelines(IList<Pipeline> pipelines);

		/// <summary>
		/// Loads the info dto for a pipeline.
		/// </summary>
		/// <param name="pipelineId">The pipeline's id</param>
		/// <returns>The pipeline's into dto if pipeline exists, otherwise null.</returns>
		Task<PipelineInfoDto> GetInfoDto(Guid pipelineId);

		/// <summary>
		/// Delete a pipeline.
		/// </summary>
		/// <param name="pipelineId">The pipeline's id.</param>
		/// <returns>True if the pipeline was deleted, otherwise false.</returns>
		Task<bool> DeletePipeline(Guid pipelineId);

		Task<PipelineInfoDto> UpdatePipeline(PipelineInfoDto pipelineDto);

		Task<IList<PipelineInfoDto>> GetDtos(string userIdentifier = default);

		/// <summary>
		/// Creates a new pipeline in the store if it not already exists.
		/// </summary>
		/// <remarks>
		/// If a pipeline with the same id already exists, values will be merged.
		/// Ignores all <c>Node</c>s provided in this object (use <c>CreateRoot</c> for adding root nodes to a pipeline).
		/// </remarks>
		/// <param name="pipeline">An object with values that will be persisted.</param>
		Task CreatePipeline(Pipeline pipeline);

		Task CreateRootOperation<TNode>(Guid pipelineId, TNode root) where TNode : Operation;

		/// <summary>
		/// Creates a new node if it does not already exist and marks it as the successor of a set of other nodes.
		/// </summary>
		/// <param name="predecessorIds">The node ids of the new nodes predecessors.</param>
		/// <param name="successor">The node that will be created.</param>
		/// <typeparam name="TNode">The node's type.</typeparam>
		Task CreateSuccessor<TNode>(IList<Guid> predecessorIds, TNode successor) where TNode : Operation;

		/// <summary>
		/// Loads a node of a specific type from the database.
		/// </summary>
		/// <param name="operationId">The node's id.</param>
		/// <typeparam name="TOperation">The specific type of the node to be loaded.</typeparam>
		/// <returns>The node or null if no node with a given id is found.</returns>
		Task<Operation> GetOperation(Guid operationId);

		Task UpdateOperation<TOperation>(TOperation operation) where TOperation : Operation;

		Task DeleteOperation(Guid operationId);

		Task<IList<OperationTuples>> GetOperationTuples();

		/// <summary>
		/// Loads a dto representing a pipeline in a format that can be directly plugged into the
		/// <a href="https://visjs.github.io/vis-network/docs/network/">vis.js</a> visualization library.
		/// </summary>
		/// <param name="pipelineId">The pipeline's id.</param>
		/// <returns>A dto holding visualization information for the pipeline.</returns>
		Task<PipelineVisualizationDto> GetVisDto(Guid pipelineId);

		Task<IList<Dataset>> GetOutputDatasets(IList<Guid> operationIds);
		Task<PipelineExport> ExportPipeline(Guid pipelineId);

		Task<IEnumerable<string>> GetUsedOperationIdentifiers(bool unique = false);
		Task<int> GetOperationCount(Guid pipelineId);
	}
}
