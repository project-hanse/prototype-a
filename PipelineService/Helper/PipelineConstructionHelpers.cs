using System;
using PipelineService.Models.Pipeline;

namespace PipelineService.Helper
{
	public static class PipelineConstructionHelpers
	{
		public static Operation Successor(Operation predecessor, Operation successor)
		{
			predecessor.Successors.Add(successor);
			if (predecessor.Outputs.Count == 1)
			{
				successor.Inputs.Add(predecessor.Outputs[0]);
			}
			else if (predecessor.Outputs.Count > 1)
			{
				throw new NotImplementedException("Multiple outputs not supported at this time.");
			}

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
