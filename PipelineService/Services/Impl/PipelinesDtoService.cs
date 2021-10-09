using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineService.Dao;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class PipelinesDtoService : IPipelinesDtoService
    {
        private readonly IPipelinesDao _pipelinesDao;
        private readonly IPipelinesExecutionDao _pipelinesExecutionDao;

        public PipelinesDtoService(IPipelinesDao pipelinesDao, IPipelinesExecutionDao pipelinesExecutionDao)
        {
            _pipelinesDao = pipelinesDao;
            _pipelinesExecutionDao = pipelinesExecutionDao;
        }

        public async Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples()
        {
            var tuples = new List<NodeTupleSingleInput>();
            var pipelines = await _pipelinesDao.GetDtos();
            foreach (var pipelineInfoDto in pipelines)
            {
                tuples.AddRange(await GetSingleInputNodeTuples(pipelineInfoDto.Id));
            }

            return tuples;
        }

        public async Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples(Guid pipelineId)
        {
            var lastExecution = await _pipelinesExecutionDao.GetLastExecutionForPipeline(pipelineId);
            if (lastExecution?.CompletedOn == null || lastExecution.Failed.Count > 0)
            {
                return new List<NodeTupleSingleInput>();
            }

            var pipeline = await _pipelinesDao.Get(pipelineId);
            var tuples = new List<NodeTupleSingleInput>();
            BuildSingleInputTuples(pipeline.Root, tuples);
            return tuples.Distinct().ToList();
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
                        OperationId = predecessor.OperationId,
                        Operation = predecessor.Operation,
                        OperationConfiguration = predecessor.OperationConfiguration,
                        TargetNodeId = singleInputNode.Id,
                        TargetOperationId = singleInputNode.OperationId,
                        TargetOperation = node.Operation,
                        Description = $"{predecessor.Operation} -> {node.Operation}"
                    });
                }
            }
        }

        public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples()
        {
            throw new NotImplementedException();
        }

        public Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples(Guid pipelineId)
        {
            throw new NotImplementedException();
        }
    }
}