using System.Threading.Tasks;

namespace PipelineService.Services;

public interface ILearningServiceClient
{
	/// <summary>
	/// Triggers the training of all models in the learning service.
	/// </summary>
	public Task TriggerModelTraining();
}
