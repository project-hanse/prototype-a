using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Helper
{
    public static class PipelineExecutionHelper
    {
        public static IList<BlockExecutionRecord> GetExecutionOrder(Pipeline pipeline)
        {
            var stack = new Stack<BlockExecutionRecord>();
            var visited = GetVisitedDictionary(pipeline);

            TopologicalSortUtil(pipeline.Root, stack, visited);

            return stack.ToList();
        }

        private static void TopologicalSortUtil(Block block, Stack<BlockExecutionRecord> stack,
            IDictionary<Guid, bool> visited)
        {
            visited[block.Id] = true;

            foreach (var blockSuccessor in block.Successors)
            {
                if (!visited[blockSuccessor.Id])
                {
                    // only visit blocks that have not been visited before
                    TopologicalSortUtil(blockSuccessor, stack, visited);
                }
            }

            stack.Push(new BlockExecutionRecord
            {
                BlockId = block.Id,
                Name = $"{block.Operation}: {JsonSerializer.Serialize(block.OperationConfiguration)}"
            });
        }

        private static IDictionary<Guid, bool> GetVisitedDictionary(Pipeline pipeline)
        {
            var visited = new Dictionary<Guid, bool>();

            AddToDict(pipeline.Root, visited);

            return visited;
        }

        private static void AddToDict(Block block, IDictionary<Guid, bool> dict)
        {
            if (!dict.ContainsKey(block.Id))
            {
                dict.Add(block.Id, false);
            }

            foreach (var blockSuccessor in block.Successors)
            {
                AddToDict(blockSuccessor, dict);
            }
        }
    }
}