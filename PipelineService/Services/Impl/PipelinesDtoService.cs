using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Dao;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class PipelinesDtoService : IPipelinesDtoService
    {
        private readonly IPipelinesDao _pipelinesesDao;
        private readonly IPipelinesExecutionDao _pipelinesExecutionDao;

        public PipelinesDtoService(IPipelinesDao pipelinesesDao, IPipelinesExecutionDao pipelinesExecutionDao)
        {
            _pipelinesesDao = pipelinesesDao;
            _pipelinesExecutionDao = pipelinesExecutionDao;
        }

        public async Task<IList<NodeTupleSingleInput>> GetSingleInputNodeTuples()
        {
	        // TODO: check if pipeline has been successfully executed
            return await _pipelinesesDao.GetTuplesSingleInput();
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
