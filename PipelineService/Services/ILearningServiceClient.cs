using System.Threading.Tasks;

namespace PipelineService.Services;

public interface ILearningServiceClient
{
	/// <summary>
	/// Triggers the async training of all models in the learning service.
	/// Also triggers the execution of all pipelines.
	/// </summary>
	public Task TriggerModelTraining();

	public Task TrainModels();
}
