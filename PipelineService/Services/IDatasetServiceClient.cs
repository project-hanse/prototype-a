using System.Threading.Tasks;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services;

public interface IDatasetServiceClient
{
	/// <summary>
	/// Loads a dataset's metadata.
	/// Might return <c>null</c> if the dataset does not exist.
	/// </summary>
	/// <param name="dataset">The dataset the metadata will be loader for.</param>
	/// <returns>The dataset's metadata or null.</returns>
	public Task<DatasetMetadataCompact> GetCompactMetadata(Dataset dataset);
}
