using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Dao;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class PipelineDtoService : IPipelineDtoService
    {
        private readonly IPipelineDao _pipelineDao;

        public PipelineDtoService(IPipelineDao pipelineDao)
        {
            _pipelineDao = pipelineDao;
        }

        public async Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples()
        {
            var tuples = new List<NodeTupleSingleInput>();
            var pipelines = await _pipelineDao.GetDtos();
            foreach (var pipelineInfoDto in pipelines)
            {
                tuples.AddRange(await GetSingleInputNodeTuples(pipelineInfoDto.Id));
            }

            return tuples;
        }

        private async Task<IEnumerable<NodeTupleSingleInput>> GetSingleInputNodeTuples(Guid pipelineId)
        {
            var pipeline = await _pipelineDao.Get(pipelineId);
            var tuples = new List<NodeTupleSingleInput>();
            BuildSingleInputTuples(pipeline.Root, tuples);
            return tuples;
        }

        private static void BuildSingleInputTuples(
            IEnumerable<Node> nodes,
            ICollection<NodeTupleSingleInput> tuples,
            Node predecessor = null)
        {
            foreach (var node in nodes)
            {
                BuildSingleInputTuples(node.Successors, tuples, node);

                if (node is NodeSingleInput singleInputNode && predecessor != null)
                {
                    tuples.Add(new NodeTupleSingleInput
                    {
                        DatasetHash = predecessor.ResultKey,
                        NodeId = predecessor.Id,
                        TargetNodeId = singleInputNode.Id,
                        Description = $"{predecessor.Operation} -> {node.Operation}"
                    });
                }
            }
        }

        public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples()
        {
            throw new NotImplementedException();
        }
    }
}