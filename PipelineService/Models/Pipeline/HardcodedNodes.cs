using System;
using System.Collections.Generic;
using PipelineService.Models.Enums;
using static PipelineService.Models.Constants.OperationIds;

namespace PipelineService.Models.Pipeline
{
	public static class HardcodedNodes
	{
		public static Operation ZamgWeatherImport(Guid pipelineId, int year)
		{
			return new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = $"ZAMG_Jahrbuch_{year}.csv",
						Store = "defaultfiles",
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_csv",
				OperationId = OpIdPdFileReadCsv,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "header", "0" },
					{ "skiprows", "4" },
					{ "skipfooter", "14" },
					{ "decimal", "," }
				}
			};
		}

		public static Operation ZamgWeatherTrim(Guid pipelineId, int year)
		{
			return new Operation
			{
				PipelineId = pipelineId,
				OperationIdentifier = "trim",
				OperationDescription = $"trim_{year}",
				OperationId = OpIdPdSingleTrim,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "first_n", "8" }
				},
			};
		}
	}
}
