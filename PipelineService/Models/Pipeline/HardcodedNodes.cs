using System;
using System.Collections.Generic;
using static PipelineService.Models.Constants.OperationIds;

namespace PipelineService.Models.Pipeline
{
	public static class HardcodedNodes
	{
		public static NodeFileInput ZamgWeatherImport(Guid pipelineId, int year)
		{
			var import = new NodeFileInput
			{
				InputObjectKey = $"ZAMG_Jahrbuch_{year}.csv",
				InputObjectBucket = "defaultfiles",
				PipelineId = pipelineId,
				Operation = "read_csv",
				OperationId = OpIdPdFileReadCsv,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "header", "0" },
					{ "skiprows", "4" },
					{ "skipfooter", "14" },
					{ "decimal", "," }
				}
			};

			return import;
		}

		public static NodeSingleInput ZamgWeatherTrim(Guid pipelineId, int year)
		{
			var trimRows = new NodeSingleInput
			{
				PipelineId = pipelineId,
				Operation = "trim",
				OperationDescription = $"trim_{year}",
				OperationId = OpIdPdSingleTrim,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "first_n", "8" }
				},
			};

			return trimRows;
		}
	}
}
