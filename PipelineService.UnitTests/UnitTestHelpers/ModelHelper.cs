using System;
using System.Collections.Generic;
using PipelineService.Helper;
using PipelineService.Models.Pipeline;

namespace PipelineService.UnitTests.UnitTestHelpers
{
    public static class ModelHelper
    {
        public static Pipeline NewDefaultPipeline(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            var cleanUp = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetId = Guid.Parse("00e61417-cada-46db-adf3-a5fc89a3b6ee"),
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "axis", "0" }
                },
            };

            var select1 = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = HashHelper.ComputeStaticHash(cleanUp),
                Operation = "select_columns",
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "0", "['Rooms', 'Bathroom', 'Landsize']" }
                }
            };

            var select2 = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = HashHelper.ComputeStaticHash(cleanUp),
                Operation = "select_columns",
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "0", "['Lattitude', 'Longtitude']" }
                }
            };

            var describe1 = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = HashHelper.ComputeStaticHash(select1),
                Operation = "describe"
            };

            var describe2 = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = HashHelper.ComputeStaticHash(select2),
                Operation = "describe"
            };

            select1.Successors.Add(describe1);
            select2.Successors.Add(describe2);
            cleanUp.Successors.Add(select1);
            cleanUp.Successors.Add(select2);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Melbourne Housing Data",
                Root = new List<Node>
                {
                    cleanUp
                }
            };
        }
    }
}