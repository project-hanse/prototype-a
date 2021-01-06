using System;
using System.Collections.Generic;
using PipelineService.Models.Pipeline;
using PipelineService.Services.Impl;

namespace PipelineService.UnitTests.UnitTestHelpers
{
    public static class ModelHelper
    {
        public static Pipeline NewDefaultPipeline(Guid pipelineId)
        {
            var cleanUp = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetId = Guid.Parse("00e61417-cada-46db-adf3-a5fc89a3b6ee"),
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                },
            };

            var select = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = HashService.ComputeStaticHash(cleanUp),
                Operation = "select_columns",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "['Rooms', 'Bathroom', 'Landsize', 'Lattitude', 'Longtitude']"}
                }
            };

            var describe = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = HashService.ComputeStaticHash(select),
                Operation = "describe"
            };

            cleanUp.Successors.Add(select);
            select.Successors.Add(describe);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Melbourne Housing Data",
                Root = cleanUp
            };
        }
    }
}