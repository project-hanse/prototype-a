using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineService.Dao;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class PipelineDtoService : IPipelineDtoService
    {
        private readonly IPipelineDao _pipelineDao;
        private readonly IPipelineExecutionDao _pipelineExecutionDao;

        public PipelineDtoService(IPipelineDao pipelineDao, IPipelineExecutionDao pipelineExecutionDao)
        {
            _pipelineDao = pipelineDao;
            _pipelineExecutionDao = pipelineExecutionDao;
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

        public async Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples(Guid pipelineId)
        {
            var lastExecution = await _pipelineExecutionDao.GetLastExecutionForPipeline(pipelineId);
            if (lastExecution?.CompletedOn == null || lastExecution.Failed.Count > 0)
            {
                return new List<NodeTupleSingleInput>();
            }

            var pipeline = await _pipelineDao.Get(pipelineId);
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

        public async Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples()
        {
            throw new NotImplementedException();
        }

        public async Task<IList<NodeTupleDoubleInput>> GetDoubleInputNodeTuples(Guid pipelineId)
        {
            throw new NotImplementedException();
        }
    }
}