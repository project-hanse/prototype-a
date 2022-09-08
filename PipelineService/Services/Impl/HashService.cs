using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
	public class HashService : IHashService
	{
		public string ComputeHash(Operation operation)
		{
			return operation.OperationHash;
		}
	}
}
