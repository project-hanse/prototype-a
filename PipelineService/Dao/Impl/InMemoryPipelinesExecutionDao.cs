using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Exceptions;
using PipelineService.Helper;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Dao.Impl
{
    public class InMemoryPipelinesExecutionDao : IPipelinesExecutionDao
    {
        private readonly ILogger<InMemoryPipelinesExecutionDao> _logger;

        private static readonly IDictionary<Guid, PipelineExecutionRecord> Store =
            new ConcurrentDictionary<Guid, PipelineExecutionRecord>();


        public InMemoryPipelinesExecutionDao(ILogger<InMemoryPipelinesExecutionDao> logger)
        {
            _logger = logger;
        }

        public Task<PipelineExecutionRecord> Create(Pipeline pipeline)
        {
            var executionRecord = new PipelineExecutionRecord
            {
                PipelineId = pipeline.Id,
                StartedOn = DateTime.UtcNow
            };

            _logger.LogInformation("Creating execution ({ExecutionId}) for pipeline {PipelineId}",
                executionRecord.Id, pipeline.Id);

            executionRecord.ToBeExecuted = PipelineExecutionHelper.GetExecutionOrder(pipeline);

            Store.Add(executionRecord.Id, executionRecord);

            return Task.FromResult(executionRecord);
        }

        public Task<PipelineExecutionRecord> Get(Guid executionId)
        {
            if (!Store.TryGetValue(executionId, out var pipelineExecutionRecord))
            {
                throw new NotFoundException("No PipelineExecutionRecord with id found");
            }

            _logger.LogInformation("Loaded execution by id {ExecutionId}", executionId);
            return Task.FromResult(pipelineExecutionRecord);
        }

        public Task<PipelineExecutionRecord> Update(PipelineExecutionRecord execution)
        {
            if (Store.ContainsKey(execution.Id))
            {
                Store.Remove(execution.Id);
            }

            Store.Add(execution.Id, execution);

            _logger.LogInformation("Updated pipeline execution record {ExecutionId}", execution.Id);

            return Task.FromResult(execution);
        }

        public Task<PipelineExecutionRecord> GetLastExecutionForPipeline(Guid pipelineId)
        {
            var record = Store.Values.FirstOrDefault(exRec => exRec.PipelineId == pipelineId);

            if (record == default)
            {
                _logger.LogInformation("No execution record found for pipeline {PipelineId}", pipelineId);
                return Task.FromResult<PipelineExecutionRecord>(null);
            }

            _logger.LogInformation("Loaded last execution record {ExecutionId} for pipeline {PipelineId}",
                record.Id, pipelineId);

            return Task.FromResult(record);
        }
    }
}