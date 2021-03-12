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
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "['Rooms', 'Bathroom', 'Landsize', 'Lattitude', 'Longtitude']"}
                }
            };

            var describe = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ResultKey,
                Operation = "describe"
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
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"method", "linear"}
                },
            };

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