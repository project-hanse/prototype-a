using PipelineService.Models.Pipeline;

namespace PipelineService.Helper
{
	public static class PipelineConstructionHelpers
	{
		/// <summary>
		/// Makes <code>successor</code> the successor of <code>node</code>. 
		/// </summary>
		public static Node Successor(Node predecessor, NodeSingleInput successor)
		{
			predecessor.Successors.Add(successor);
			successor.InputDatasetHash = predecessor.ResultKey;
			return successor;
		}

		/// <summary>
		/// Makes <code>successor</code> the successor of <code>node</code>. 
		/// </summary>
		public static Node Successor(NodeFileInput predecessor, NodeSingleInput successor)
		{
			predecessor.Successors.Add(successor);
			successor.InputDatasetHash = predecessor.ResultKey;
			return successor;
		}

		public static Node Successor(Node predecessor1, Node predecessor2, NodeDoubleInput successor)
		{
			predecessor1.Successors.Add(successor);
			predecessor2.Successors.Add(successor);
			successor.InputDatasetOneHash = predecessor1.ResultKey;
			successor.InputDatasetTwoHash = predecessor2.ResultKey;
			return successor;
		}
	}
}
