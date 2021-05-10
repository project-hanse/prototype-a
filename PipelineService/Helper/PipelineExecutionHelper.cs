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
        public static IList<NodeExecutionRecord> GetExecutionOrder(Pipeline pipeline)
        {
            var stack = new Stack<NodeExecutionRecord>();
            var visited = GetVisitedDictionary(pipeline);

            foreach (var root in pipeline.Root)
            {
                TopologicalSortUtil(root, stack, visited);
            }

            return stack.ToList();
        }

        private static void TopologicalSortUtil(Node node, Stack<NodeExecutionRecord> stack,
            IDictionary<Guid, bool> visited, int level = 0)
        {
            visited[node.Id] = true;

            level++;
            foreach (var blockSuccessor in node.Successors)
            {
                if (!visited[blockSuccessor.Id])
                {
                    // only visit blocks that have not been visited before
                    TopologicalSortUtil(blockSuccessor, stack, visited, level);
                }
            }

            stack.Push(new NodeExecutionRecord
            {
                PipelineId = node.PipelineId,
                NodeId = node.Id,
                Node = node,
                Name = $"{node.Operation}:{JsonSerializer.Serialize(node.OperationConfiguration)}",
                Level = level
            });
        }

        private static IDictionary<Guid, bool> GetVisitedDictionary(Pipeline pipeline)
        {
            var visited = new Dictionary<Guid, bool>();

            foreach (var root in pipeline.Root)
            {
                AddToDict(root, visited);
            }

            return visited;
        }

        private static void AddToDict(Node node, IDictionary<Guid, bool> dict)
        {
            if (!dict.ContainsKey(node.Id))
            {
                dict.Add(node.Id, false);
            }

            foreach (var blockSuccessor in node.Successors)
            {
                AddToDict(blockSuccessor, dict);
            }
        }
    }
}