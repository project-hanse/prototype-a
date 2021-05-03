using System;
using System.Collections.Generic;

namespace PipelineService.Models.Pipeline
{
    /// <summary>
    /// Generates a set of default pipelines with hardcoded dataset ids for prototyping.
    /// </summary>
    public static class HardcodedDefaultPipelines
    {
        private static readonly Guid DsIdMelbourneHousingFull = Guid.Parse("00e61417-cada-46db-adf3-a5fc89a3b6ee");
        private static readonly Guid DsIdMelbourneHousePricesLess = Guid.Parse("0c2acbdb-544b-4efc-ae54-c2dcba988654");
        private static readonly Guid DsIdInfluencaVienna20092018 = Guid.Parse("4cfd0698-004a-404e-8605-de2f830190f2");

        public static Pipeline MelbourneHousingPipeline(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            var cleanUp = new SimpleNode
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdMelbourneHousingFull,
                Operation = "dropna",
                OperationId = Guid.Parse("0759dede-2cee-433c-b314-10a8fa456e62"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                },
            };

            var select = new SimpleNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = cleanUp.ResultKey,
                Operation = "select_columns",
                OperationId = Guid.Parse("7b0bb47f-f997-43d8-acb1-c31f2a22475d"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "[1, 3, 4, 5]"}
                }
            };

            var describe = new SimpleNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ResultKey,
                Operation = "describe",
                OperationId = Guid.Parse("0759dede-2cee-433c-b314-10a8fa456e62"),
            };

            cleanUp.Successors.Add(select);
            select.Successors.Add(describe);

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

        public static Pipeline InfluenzaInterpolation(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            var interpolate = new SimpleNode
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdInfluencaVienna20092018,
                Operation = "interpolate",
                OperationId = Guid.Parse("0759dede-2cee-433c-b314-10a8fa456e62"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"method", "linear"}
                },
            };

            var select = new SimpleNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = interpolate.ResultKey,
                Operation = "select_columns",
                OperationId = Guid.Parse("7b0bb47f-f997-43d8-acb1-c31f2a22475d"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "[0, 1]"}
                }
            };

            interpolate.Successors.Add(select);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Influenza Interpolation",
                Root = new List<Node>
                {
                    interpolate
                }
            };
        }

        public static Pipeline MelbourneHousingPipelineWithError(Guid pipelineId = default)
        {
            var pipeline = MelbourneHousingPipeline(pipelineId);
            pipeline.Name = "Invalid: Melbourne Housing Data";

            var unknownOperation = new SimpleNode
            {
                PipelineId = pipeline.Id,
                InputDatasetId = DsIdMelbourneHousePricesLess,
                Operation = "unknown_operation",
                OperationId = Guid.NewGuid(), // random guid that does not represent an operation
                OperationConfiguration = new Dictionary<string, string>()
            };

            var describe = new SimpleNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = unknownOperation.ResultKey,
                Operation = "describe",
                OperationId = Guid.Parse("0759dede-2cee-433c-b314-10a8fa456e62"),
            };

            unknownOperation.Successors.Add(describe);
            pipeline.Root[0].Successors.Add(unknownOperation);
            return pipeline;
        }
    }
}