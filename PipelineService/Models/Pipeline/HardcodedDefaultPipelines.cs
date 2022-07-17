using System;
using System.Collections.Generic;
using PipelineService.Extensions;
using PipelineService.Helper;
using PipelineService.Models.Enums;
using static PipelineService.Models.Constants.OperationIds;

namespace PipelineService.Models.Pipeline
{
	/// <summary>
	/// Generates a set of default pipelines with hardcoded dataset ids for prototyping.
	/// </summary>
	public static class HardcodedDefaultPipelines
	{
		public static IList<Pipeline> NewDefaultPipelines()
		{
			return new List<Pipeline>
			{
				MelbourneHousingPipeline(),
				InfluenzaInterpolation(),
				MelbourneHousingPipelineWithError(),
				ChemnitzStudentAndJobsPipeline(),
				SimulatedVineYieldPipeline(),
				ZamgWeatherPreprocessingGraz(),
				ZamgWeatherPreprocessingGraz(Guid.NewGuid(), 1991),
				BeerProductionAustralia()
			};
		}

		public static IList<Pipeline> PipelineTemplates()
		{
			return new List<Pipeline>
			{
				MelbourneHousingPipeline(Guid.Parse("d10eb9ef-292d-49ed-be8d-486970c0cf8e")),
				InfluenzaInterpolation(Guid.Parse("14d110b2-5125-43e9-8db4-617ea330af87")),
				MelbourneHousingPipelineWithError(Guid.Parse("8c5bcfe6-7f7b-4e9a-9656-9f0ca45fa236")),
				ChemnitzStudentAndJobsPipeline(Guid.Parse("5d3ab062-ea9b-43b9-ada0-4bb0f3035be1")),
				SimulatedVineYieldPipeline(Guid.Parse("d4702c80-53b5-4f3d-b4e1-d79dd859d9ec")),
				ZamgWeatherPreprocessingGraz(Guid.Parse("6490fdbc-0240-4a4e-8c36-fca40b89f80e")),
				ZamgWeatherPreprocessingGraz(Guid.Parse("40a61687-794c-4fab-9c17-5608833b0f33"), 1991),
				BeerProductionAustralia(Guid.Parse("ce01a52c-daa2-4495-af5b-5aaa4c30c6b3")),
				EmptyTemplate()
			};
		}

		public static Pipeline EmptyTemplate()
		{
			return new()
			{
				Id = Guid.Parse("4bfd7879-c86a-4597-89f1-941a9fed9e4f"),
				Name = "Empty Pipeline",
				Root = new List<Operation>()
			};
		}

		public static Pipeline MelbourneHousingPipeline(Guid pipelineId = default)
		{
			if (pipelineId == default)
			{
				pipelineId = Guid.NewGuid();
			}

			var import = new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = "Melbourne_housing_FULL.csv",
						Store = "defaultfiles"
					}
				},
				Outputs = new List<Dataset>
				{
					new()
					{
						Type = DatasetType.PdSeries,
						Store = "dataframes",
						Key = Guid.NewGuid().ToString()
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_csv",
				OperationId = OpIdPdFileReadCsv,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "header", "0" }
				}
			};

			var cleanUp = new Operation
			{
				PipelineId = pipelineId,
				OperationIdentifier = "dropna",
				OperationId = OpIdPdSingleGeneric,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "axis", "0" }
				},
				Outputs = new List<Dataset>
				{
					new()
					{
						Type = DatasetType.PdSeries,
						Store = "dataframes"
					}
				},
			};

			PipelineConstructionHelpers.Successor(import, cleanUp);

			var select = new Operation
			{
				PipelineId = pipelineId,
				OperationIdentifier = "select_columns",
				OperationId = OpIdPdSingleSelectColumns,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "columns", "['Price', 'YearBuilt', 'BuildingArea', 'Landsize']" }
				},
				Outputs = new List<Dataset>
				{
					new()
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
			};

			PipelineConstructionHelpers.Successor(cleanUp, select);

			var describe = new Operation
			{
				PipelineId = pipelineId,
				OperationIdentifier = "describe",
				OperationId = OpIdPdSingleGeneric,
				Outputs = new List<Dataset>
				{
					new()
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
			};

			PipelineConstructionHelpers.Successor(select, describe);

			return new Pipeline
			{
				Id = pipelineId,
				Name = "Melbourne Housing Data",
				Root = new List<Operation>
				{
					import
				}
			};
		}

		public static Pipeline InfluenzaInterpolation(Guid pipelineId = default)
		{
			if (pipelineId == default)
			{
				pipelineId = Guid.NewGuid();
			}

			var import = new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = "influenca_vienna_2009-2018.csv",
						Store = "defaultfiles"
					}
				},
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_csv",
				OperationId = OpIdPdFileReadCsv,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "header", "0" }
				}
			};

			var interpolate = new Operation
			{
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "interpolate",
				OperationId = OpIdPdSingleInterpolate,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "axis", "0" },
					{ "method", "linear" }
				},
			};

			PipelineConstructionHelpers.Successor(import, interpolate);

			var select = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "select_columns",
				OperationId = OpIdPdSingleSelectColumns,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "columns", "['year', 'week', 'weekly_infections']" }
				}
			};

			PipelineConstructionHelpers.Successor(interpolate, select);

			var describe = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "describe",
				OperationId = OpIdPdSingleGeneric,
			};

			PipelineConstructionHelpers.Successor(select, describe);

			return new Pipeline
			{
				Id = pipelineId,
				Name = "Influenza Interpolation",
				Root = new List<Operation>
				{
					import
				}
			};
		}

		public static Pipeline MelbourneHousingPipelineWithError(Guid pipelineId = default)
		{
			var pipeline = MelbourneHousingPipeline(pipelineId);
			pipeline.Name = "Invalid: Melbourne Housing Data";

			var unknownOperation = new Operation
			{
				PipelineId = pipeline.Id,
				OperationIdentifier = "unknown_operation",
				OperationId = Guid.NewGuid(), // random guid that does not represent an operation
				OperationConfiguration = new Dictionary<string, string>()
			};

			var describe = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "describe",
				OperationId = OpIdPdSingleGeneric,
			};
			PipelineConstructionHelpers.Successor(unknownOperation, describe);


			pipeline.Root[0].Successors.Add(unknownOperation);
			return pipeline;
		}

		public static Pipeline ChemnitzStudentAndJobsPipeline(Guid pipelineId = default)
		{
			if (pipelineId == default)
			{
				pipelineId = Guid.NewGuid();
			}

			var import1Berufsbildung = new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = "21211-003Z_format.csv",
						Store = "defaultfiles"
					}
				},
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_csv",
				OperationId = OpIdPdFileReadCsv
			};

			// Data cleaning Berufsbildung
			var trim1Berufsbildung = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "trim",
				OperationId = OpIdPdSingleTrim,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "first_n", "6" }
				},
			};

			PipelineConstructionHelpers.Successor(import1Berufsbildung, trim1Berufsbildung);

			var resetIndexBerufsbildung = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "reset_index",
				OperationId = OpIdPdSingleResetIndex
			};

			PipelineConstructionHelpers.Successor(trim1Berufsbildung, resetIndexBerufsbildung);

			var setHeaderBerufsbildung = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "set_header",
				OperationId = OpIdPdSingleMakeColumnHeader,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "header_row", "0" }
				}
			};

			PipelineConstructionHelpers.Successor(resetIndexBerufsbildung, setHeaderBerufsbildung);

			var dropNaBerufsbildung = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "dropna",
				OperationId = OpIdPdSingleGeneric,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "axis", "0" }
				},
			};

			PipelineConstructionHelpers.Successor(setHeaderBerufsbildung, dropNaBerufsbildung);

			var trim2Berufsbildung = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "trim",
				OperationId = OpIdPdSingleTrim,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "first_n", "1" }
				},
			};

			PipelineConstructionHelpers.Successor(dropNaBerufsbildung, trim2Berufsbildung);

			var selectRowsBerufsbildung = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "select",
				OperationId = OpIdPdSingleSelectRows,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "select_value", "Insgesamt" },
					{ "column_name", "Typ" }
				},
			};

			PipelineConstructionHelpers.Successor(trim2Berufsbildung, selectRowsBerufsbildung);

			var import1Studenten = new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = "21311-001Z_format.csv",
						Store = "defaultfiles"
					}
				},
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_csv",
				OperationId = OpIdPdFileReadCsv
			};
			// Data Cleaning Studenten
			var trim1Studenten = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "trim",
				OperationId = OpIdPdSingleTrim,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "first_n", "6" }
				},
			};

			PipelineConstructionHelpers.Successor(import1Studenten, trim1Studenten);

			var resetIndexStudenten = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "reset_index",
				OperationId = OpIdPdSingleResetIndex
			};

			PipelineConstructionHelpers.Successor(trim1Studenten, resetIndexStudenten);

			var setHeaderStudenten = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "set_header",
				OperationId = OpIdPdSingleMakeColumnHeader,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "header_row", "0" }
				}
			};

			PipelineConstructionHelpers.Successor(resetIndexStudenten, setHeaderStudenten);

			var dropNaStudenten = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "dropna",
				OperationId = OpIdPdSingleGeneric,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "axis", "0" }
				},
			};
			PipelineConstructionHelpers.Successor(setHeaderStudenten, dropNaStudenten);

			var trim2Studenten = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "trim",
				OperationId = OpIdPdSingleTrim,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "first_n", "1" }
				},
			};
			PipelineConstructionHelpers.Successor(dropNaStudenten, trim2Studenten);

			var selectRowsStudenten = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "select",
				OperationId = OpIdPdSingleSelectRows,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "select_value", "Insgesamt" },
					{ "column_name", "Typ" }
				},
			};
			PipelineConstructionHelpers.Successor(trim2Studenten, selectRowsStudenten);

			var join = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "join",
				OperationId = OpIdPdDoubleJoin,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "on", "Jahr" },
					{ "lsuffix", "_berufsbildung" },
					{ "rsuffix", "_studenten" }
				}
			};
			PipelineConstructionHelpers.Successor(selectRowsStudenten, selectRowsBerufsbildung, join);

			var describe = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "describe",
				OperationId = OpIdPdSingleGeneric,
			};

			PipelineConstructionHelpers.Successor(join, describe);

			return new Pipeline
			{
				Id = pipelineId,
				Name = "Chemnitz Students and Jobs",
				Root = new List<Operation>
				{
					import1Berufsbildung,
					import1Studenten
				}
			};
		}

		public static Pipeline SimulatedVineYieldPipeline(Guid pipelineId = default)
		{
			if (pipelineId == default)
			{
				pipelineId = Guid.NewGuid();
			}

			var import = new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = "simulated-vine-yield-styria.xlsx",
						Store = "defaultfiles"
					}
				},
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_excel",
				OperationId = OpIdPdFileReadExcel,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "skiprows", "1" }
				}
			};

			var renameLabels = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "rename",
				OperationId = OpIdPdSingleRename,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "mapper", "{'Unnamed: 0':'Jahr'}" },
					{ "axis", "columns" }
				}
			};

			PipelineConstructionHelpers.Successor(import, renameLabels);

			var setIndex = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "set_index",
				OperationId = OpIdPdSingleSetIndex,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "keys", "Jahr" }
				}
			};

			PipelineConstructionHelpers.Successor(renameLabels, setIndex);

			var mean = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "mean",
				OperationId = OpIdPdSingleMean,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "axis", "1" }
				}
			};

			PipelineConstructionHelpers.Successor(setIndex, mean);

			return new Pipeline
			{
				Id = pipelineId,
				Name = "Simulated Vine Yield Styria",
				Root = new List<Operation>
				{
					import
				}
			};
		}

		public static Pipeline BeerProductionAustralia(Guid pipelineId = default)
		{
			if (pipelineId == default)
			{
				pipelineId = Guid.NewGuid();
			}


			var pipeline = new Pipeline
			{
				Name = "Beer Production Australia 1956 - 1995",
				Id = pipelineId
			};

			var import = new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = "monthly-beer-production-in-australia.csv",
						Store = "defaultfiles"
					}
				},
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_csv",
				OperationId = OpIdPdFileReadCsv,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "separator", "," },
					{ "decimal", "." },
					{ "parse_dates", "[]" },
					{ "index_col", "None" }
				}
			};

			var setDateIndex = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "set_date_index",
				OperationDescription = "set_date_index",
				OperationId = OpIdPdSingleSetDateIndex,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "col_name", "Month" },
					{ "format", "%Y-%m" }
				}
			};

			PipelineConstructionHelpers.Successor(import, setDateIndex);

			var plotHistory = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.StaticPlot,
						Store = "plots",
						Key = $"{Guid.NewGuid()}.svg"
					}
				},
				OperationIdentifier = "plot",
				OperationDescription = "plot",
				OperationId = OpIdCustomPlotDfMatPlotLib,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "ax", "None" },
					{ "grid", "None" },
					{ "kind", "line" },
					{ "layout", "None" },
					{ "legend", "true" },
					{ "sharex", "false" },
					{ "sharey", "false" },
					{ "style", "{}" },
					{ "subplots", "false" },
					{ "title", pipeline.Name },
					{ "use_index", "true" },
					{ "x", "Month" },
					{ "y", "None" }
				}
			};

			PipelineConstructionHelpers.Successor(setDateIndex, plotHistory);

			var rename = new Operation()
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "rename",
				OperationDescription = "rename",
				OperationId = OpIdPdSingleGeneric,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "mapper", "None" },
					{ "index", "None" },
					{ "columns", "{'Month': 'ds', 'Monthly beer production':'y'}" },
					{ "axis", "None" },
					{ "copy", "True" },
					{ "level", "None" },
					{ "errors", "ignore" }
				}
			};

			PipelineConstructionHelpers.Successor(import, rename);

			var fit = new Operation()
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.Prophet,
						Store = "fit",
						Key = $"{Guid.NewGuid()}.prophet"
					}
				},
				OperationIdentifier = "fit",
				OperationDescription = "fit",
				OperationId = OpIdProphetFit,
				OperationConfiguration = new Dictionary<string, string>()
			};

			PipelineConstructionHelpers.Successor(rename, fit);

			var makeFuture = new Operation()
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "make_future_dataframe",
				OperationDescription = "make_future_dataframe",
				OperationId = OpIdProphetMakeFuture,
				OperationConfiguration = new Dictionary<string, string>()
				{
					{ "periods", "18" },
					{ "freq", "M" },
					{ "include_history", "true" }
				}
			};

			PipelineConstructionHelpers.Successor(fit, makeFuture);

			var predict = new Operation()
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "predict",
				OperationDescription = "predict",
				OperationId = OpIdProphetPredict,
				OperationConfiguration = new Dictionary<string, string>()
			};

			PipelineConstructionHelpers.Successor(fit, makeFuture, predict);

			var plotPrediction = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.StaticPlot,
						Store = "plots",
						Key = $"{Guid.NewGuid()}.svg"
					}
				},
				OperationIdentifier = "plot",
				OperationDescription = "plot_prediction",
				OperationId = OpIdCustomPlotDfMatPlotLib,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "ax", "None" },
					{ "grid", "None" },
					{ "kind", "line" },
					{ "layout", "None" },
					{ "legend", "true" },
					{ "sharex", "false" },
					{ "sharey", "false" },
					{ "style", "{}" },
					{ "subplots", "false" },
					{ "title", "Forecast Beer Production 18 Months" },
					{ "use_index", "true" },
					{ "x", "None" },
					{ "y", "None" }
				}
			};

			PipelineConstructionHelpers.Successor(predict, plotPrediction);

			pipeline.Root.Add(import);

			return pipeline;
		}

		public static Pipeline ZamgWeatherPreprocessingGraz(Guid pipelineId = default, int to = 2020)
		{
			if (pipelineId == default)
			{
				pipelineId = Guid.NewGuid();
			}

			var pipeline = new Pipeline
			{
				Name = $"ZAMG Weather Data Preprocessing Graz 1990-{to}",
				Id = pipelineId
			};

			var nodesToJoin = new Queue<Operation>();
			for (var year = 1990; year <= to; year++)
			{
				var import = HardcodedNodes.ZamgWeatherImport(pipelineId, year);
				var transpose = new Operation
				{
					PipelineId = pipelineId,
					Outputs =
					{
						new Dataset
						{
							Type = DatasetType.PdDataFrame,
							Store = "dataframes"
						}
					},
					OperationIdentifier = "transpose",
					OperationDescription = $"transpose_{year}",
					OperationId = OpIdPdSingleTranspose
				};
				var trim = HardcodedNodes.ZamgWeatherTrim(pipelineId, year);
				var setHeader = new Operation
				{
					PipelineId = pipelineId,
					Outputs =
					{
						new Dataset
						{
							Type = DatasetType.PdDataFrame,
							Store = "dataframes"
						}
					},
					OperationIdentifier = "set_header",
					OperationDescription = $"set_header_{year}",
					OperationId = OpIdPdSingleMakeColumnHeader,
					OperationConfiguration = new Dictionary<string, string>
					{
						{ "header_row", "Parameter" }
					}
				};
				var setDateIndex = new Operation
				{
					PipelineId = pipelineId,
					Outputs =
					{
						new Dataset
						{
							Type = DatasetType.PdDataFrame,
							Store = "dataframes"
						}
					},
					OperationIdentifier = "set_date_index",
					OperationDescription = $"set_date_index_{year}",
					OperationId = OpIdPdSingleSetDateIndex
				};
				PipelineConstructionHelpers.Successor(import, transpose);
				PipelineConstructionHelpers.Successor(transpose, trim);
				PipelineConstructionHelpers.Successor(trim, setHeader);
				PipelineConstructionHelpers.Successor(setHeader, setDateIndex);
				pipeline.Root.Add(import);
				nodesToJoin.Enqueue(setDateIndex);
			}

			// join nodes
			while (nodesToJoin.Count >= 2)
			{
				var operation1 = nodesToJoin.Dequeue();
				var operation2 = nodesToJoin.Dequeue();
				var concatOperation = new Operation
				{
					PipelineId = pipelineId,
					Outputs =
					{
						new Dataset
						{
							Type = DatasetType.PdDataFrame,
							Store = "dataframes"
						}
					},
					OperationIdentifier = "concat",
					OperationDescription =
						$"concat_{operation1.OperationIdentifier.LastChars(4)}_{operation2.OperationIdentifier.LastChars(4)}",
					OperationId = OpIdPdDoubleConcat
				};
				PipelineConstructionHelpers.Successor(operation1, operation2, concatOperation);

				nodesToJoin.Enqueue(concatOperation);
			}

			var sortIndex = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "sort_index",
				OperationId = OpIdPdSingleSortIndex
			};

			PipelineConstructionHelpers.Successor(nodesToJoin.Dequeue(), sortIndex);

			var replaceStrings = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "replace",
				OperationId = OpIdPdSingleReplace,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "to_replace", "['Spuren']" },
					{ "value", "0" }
				}
			};

			PipelineConstructionHelpers.Successor(sortIndex, replaceStrings);

			var toNumeric = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "to_numeric",
				OperationId = OpIdPdSingleDfToNumeric,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "errors", "coerce" }
				}
			};

			PipelineConstructionHelpers.Successor(replaceStrings, toNumeric);

			var interpolate = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "interpolate",
				OperationId = OpIdPdSingleInterpolate,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "axis", "0" },
					{ "method", "time" },
				}
			};

			PipelineConstructionHelpers.Successor(toNumeric, interpolate);

			var resample = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "resample",
				OperationId = OpIdPdSingleResample,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "rule", "1Y" },
					{ "group_by_operation", "mean" },
				}
			};

			PipelineConstructionHelpers.Successor(interpolate, resample);

			var trimFirstYears = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "trim",
				OperationId = OpIdPdSingleTrim,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "first_n", "18" }
				},
			};

			PipelineConstructionHelpers.Successor(resample, trimFirstYears);

			var dropEmptyColumns = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "dropna",
				OperationId = OpIdPdSingleGeneric,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "axis", "1" }
				},
			};

			PipelineConstructionHelpers.Successor(trimFirstYears, dropEmptyColumns);

			// Simulated Vine Yield
			var importVine = new Operation
			{
				Inputs =
				{
					new Dataset
					{
						Type = DatasetType.File,
						Key = "simulated-vine-yield-styria.xlsx",
						Store = "defaultfiles"
					}
				},
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				PipelineId = pipelineId,
				OperationIdentifier = "read_excel",
				OperationId = OpIdPdFileReadExcel,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "skiprows", "1" }
				}
			};

			var renameLabels = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "rename",
				OperationId = OpIdPdSingleRename,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "mapper", "{'Unnamed: 0':'Jahr'}" },
					{ "axis", "columns" }
				}
			};

			PipelineConstructionHelpers.Successor(importVine, renameLabels);

			var setIndex = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "set_index",
				OperationId = OpIdPdSingleSetIndex,
				OperationConfiguration = new Dictionary<string, string>
				{
					{ "keys", "Jahr" }
				}
			};

			PipelineConstructionHelpers.Successor(renameLabels, setIndex);

			var predict = new Operation
			{
				PipelineId = pipelineId,
				Outputs =
				{
					new Dataset
					{
						Type = DatasetType.PdDataFrame,
						Store = "dataframes"
					}
				},
				OperationIdentifier = "predict_yield",
				OperationId = OpIdSklearnDoubleMlpRegr
			};

			PipelineConstructionHelpers.Successor(dropEmptyColumns, setIndex, predict);
			pipeline.Root.Add(importVine);
			return pipeline;
		}
	}
}
