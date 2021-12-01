using PipelineService.Models.Pipeline;

namespace PipelineService.Extensions
{
	public static class OperationExtensions
	{
		public static Operation CalculateOutputKey(this Operation operation)
		{
			operation.Output.Key = operation.ComputedHash;
			return operation;
		}
	}
}
