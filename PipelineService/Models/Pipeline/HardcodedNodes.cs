using System;
using System.Collections.Generic;
using PipelineService.Models.Constants;
using static PipelineService.Models.Constants.OperationIds;

namespace PipelineService.Models.Pipeline
{
    public static class HardcodedNodes
    {
        public static Node ZamgWeatherPreprocessing(Guid pipelineId, int year)
        {
            var trimRows = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetId = DatasetIds.ZamgWeatherId(year),
                Operation = $"trim_{year}",
                OperationId = OpIdPdSingleDrop,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"labels", "['Beaufort']"},
                    // {"last_n", "13"}
                },
            };
            return trimRows;
        }
    }
}