using PipelineService.Extensions;
using PipelineService.Models.Pipeline;

namespace PipelineService.Helper
{
	public static class PipelineConstructionHelpers
	{
		public static Operation Successor(Operation predecessor, Operation successor)
		{
			predecessor.Successors.Add(successor);
			predecessor.CalculateOutputKey();
			successor.Inputs.Add(predecessor.Output);
			return successor;
		}

		public static Operation Successor(Operation predecessor1, Operation predecessor2, Operation successor)
		{
			Successor(predecessor1, successor);
			Successor(predecessor2, successor);
			return successor;
		}
	}
}
