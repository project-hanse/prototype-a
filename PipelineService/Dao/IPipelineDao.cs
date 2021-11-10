using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao
{
	public interface IPipelineDao
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

		Task<PipelineInfoDto> GetInfoDto(Guid pipelineId);

		Task<IList<PipelineInfoDto>> GetDtos();

		/// <summary>
		/// Creates a new pipeline in the store if it not already exists.
		/// </summary>
		/// <remarks>
		/// If a pipeline with the same id already exists, values will be merged.
		/// Ignores all <c>Node</c>s provided in this object (use <c>CreateRoot</c> for adding root nodes to a pipeline).
		/// </remarks>
		/// <param name="pipeline">An object with values that will be persisted.</param>
		Task CreatePipeline(Pipeline pipeline);

		Task CreateRootNode<TN>(Guid pipelineId, TN root) where TN : Node;

		Task CreateSuccessor<TP, TS>(Guid predecessorId, TS successor) where TP : Node where TS : Node;

		/// <summary>
		/// Loads a node from the database.
		/// The node's type will be automatically detected.
		/// </summary>
		/// <remarks>
		/// Hint: if you want to load a node of a specific type, use <c>LoadNode&lt;TN&gt;</c> instead.
		/// </remarks>
		/// <param name="nodeId">The node's id.</param>
		/// <returns>The node or null if no node with a given id is found.</returns>
		Task<Node> GetNode(Guid nodeId);

		/// <summary>
		/// Loads a node of a specific type from the database.
		/// </summary>
		/// <param name="nodeId">The node's id.</param>
		/// <typeparam name="T">The specific type of the node to be loaded.</typeparam>
		/// <returns>The node or null if no node with a given id is found.</returns>
		Task<T> GetNode<T>(Guid nodeId) where T : Node;

		Task<IList<NodeTupleSingleInput>> GetTuplesSingleInput();

		/// <summary>
		/// Loads a dto representing a pipeline in a format that can be directly plugged into the
		/// <a href="https://visjs.github.io/vis-network/docs/network/">vis.js</a> visualization library.
		/// </summary>
		/// <param name="pipelineId">The pipeline's id.</param>
		/// <returns>A dto holding visualization information for the pipeline.</returns>
		Task<PipelineVisualizationDto> GetVisDto(Guid pipelineId);
	}
}
