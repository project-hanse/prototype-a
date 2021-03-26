using System;
using System.Collections.Generic;

namespace PipelineService.Models.Pipeline
{
    /// <summary>
    /// Generates a set of default pipelines with hardcoded dataset ids for prototyping.
    /// </summary>
    public static class HardcodedDefaultPipelines
    {
        public static Pipeline MelbourneHousingPipeline(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            var cleanUp = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetId = Guid.Parse("00e61417-cada-46db-adf3-a5fc89a3b6ee"),
                Operation = "dropna",
                OperationId = Guid.Parse("0759dede-2cee-433c-b314-10a8fa456e62"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                },
            };

            var select = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = cleanUp.ResultKey,
                Operation = "select_columns",
                OperationId = Guid.Parse("7b0bb47f-f997-43d8-acb1-c31f2a22475d"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "['Rooms', 'Bathroom', 'Landsize', 'Lattitude', 'Longtitude']"}
                }
            };

            var describe = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ResultKey,
                Operation = "describe",
                OperationId = Guid.Parse("78f3b2f5-958c-49e9-b0cb-be0fd762ffa9"),
            };

            cleanUp.Successors.Add(select);
            select.Successors.Add(describe);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Melbourne Housing Data",
                Root = new List<Block>
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

            var interpolate = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetId = Guid.Parse("4cfd0698-004a-404e-8605-de2f830190f2"),
                Operation = "interpolate",
                OperationId = Guid.Parse("0da055fe-0a0c-41fa-a6d6-768415dc834a"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"method", "linear"}
                },
            };

            var select = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = interpolate.ResultKey,
                Operation = "select_columns",
                OperationId = Guid.Parse("7b0bb47f-f997-43d8-acb1-c31f2a22475d"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "['weekly_infections']"}
                }
            };

            interpolate.Successors.Add(select);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Influenza Interpolation",
                Root = new List<Block>
                {
                    interpolate
                }
            };
        }
    }
}