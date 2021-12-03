using PipelineService.Models.Pipeline;

namespace PipelineService.Services
{
	public interface IHashService
	{
		/// <summary>
		/// Computes a hash value from a given node's operation, configuration, and input datasets (id or input hash).
		/// </summary>
		/// <param name="operation node a hash should be calculated for.</param>
		/// <returns>The computes hash as a string</returns>
		public string ComputeHash(Operation operation);
	}
}
