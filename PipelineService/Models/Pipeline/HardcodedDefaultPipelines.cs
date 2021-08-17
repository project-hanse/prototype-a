using System;
using System.Collections.Generic;
using static PipelineService.Models.Constants.DatasetIds;
using static PipelineService.Models.Constants.OperationIds;

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

            var import = new NodeFileInput
            {
                InputObjectKey = "Melbourne_housing_FULL.csv",
                InputObjectBucket = "defaultfiles",
                PipelineId = pipelineId,
                Operation = "read_csv",
                OperationId = OpIdPdFileInputReadCsv,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "header", "0" }
                }
            };

            var cleanUp = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "dropna",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "axis", "0" }
                },
            };

            var select = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = cleanUp.ResultKey,
                Operation = "select_columns",
                OperationId = Guid.Parse("7b0bb47f-f997-43d8-acb1-c31f2a22475d"),
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "0", "['Price', 'YearBuilt', 'BuildingArea', 'Landsize']" }
                }
            };

            var describe = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ResultKey,
                Operation = "describe",
                OperationId = OpIdPdSingleGeneric,
            };

            Successor(import, cleanUp);
            Successor(cleanUp, select);
            Successor(select, describe);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Melbourne Housing Data",
                Root = new List<Node>
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

            var interpolate = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdInfluencaVienna20092018,
                Operation = "interpolate",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "method", "linear" }
                },
            };

            var select = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = interpolate.ResultKey,
                Operation = "select_columns",
                OperationId = OpIdPdSingleSelectColumns,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "0", "['year', 'week', 'weekly_infections']" }
                }
            };

            interpolate.Successors.Add(select);

            var describe = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ResultKey,
                Operation = "describe",
                OperationId = OpIdPdSingleGeneric,
            };
            select.Successors.Add(describe);

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

            var unknownOperation = new NodeSingleInput
            {
                PipelineId = pipeline.Id,
                InputDatasetId = DsIdMelbourneHousePricesLess,
                Operation = "unknown_operation",
                OperationId = Guid.NewGuid(), // random guid that does not represent an operation
                OperationConfiguration = new Dictionary<string, string>()
            };

            var describe = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = unknownOperation.ResultKey,
                Operation = "describe",
                OperationId = OpIdPdSingleGeneric,
            };

            unknownOperation.Successors.Add(describe);
            pipeline.Root[0].Successors.Add(unknownOperation);
            return pipeline;
        }

        public static Pipeline ChemnitzStudentAndJobsPipeline(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            // Data cleaning Berufsbildung
            var trim1Berufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdChemnitzBerufsbildung1993,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "first_n", "6" }
                },
            };

            var setHeaderBerufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = trim1Berufsbildung.ResultKey,
                Operation = "set_header",
                OperationId = OpIdPdSingleMakeColumnHeader,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "header_row", "0" }
                }
            };
            trim1Berufsbildung.Successors.Add(setHeaderBerufsbildung);

            var dropNaBerufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = setHeaderBerufsbildung.ResultKey,
                Operation = "dropna",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "axis", "0" }
                },
            };
            setHeaderBerufsbildung.Successors.Add(dropNaBerufsbildung);

            var trim2Berufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = dropNaBerufsbildung.ResultKey,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "first_n", "1" }
                },
            };
            dropNaBerufsbildung.Successors.Add(trim2Berufsbildung);


            var selectRowsBerufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = trim2Berufsbildung.ResultKey,
                Operation = "select",
                OperationId = OpIdPdSingleSelectRows,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "select_value", "Insgesamt" },
                    { "column_name", "Typ" }
                },
            };
            trim2Berufsbildung.Successors.Add(selectRowsBerufsbildung);

            // Data Cleaning Studenten
            var trim1Studenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdChemnitzStudenten1993,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "first_n", "6" }
                },
            };

            var setHeaderStudenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = trim1Studenten.ResultKey,
                Operation = "set_header",
                OperationId = OpIdPdSingleMakeColumnHeader,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "header_row", "0" }
                }
            };
            trim1Studenten.Successors.Add(setHeaderStudenten);

            var dropNaStudenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = setHeaderStudenten.ResultKey,
                Operation = "dropna",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "axis", "0" }
                },
            };
            setHeaderStudenten.Successors.Add(dropNaStudenten);

            var trim2Studenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = dropNaStudenten.ResultKey,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "first_n", "1" }
                },
            };
            dropNaStudenten.Successors.Add(trim2Studenten);

            var selectRowsStudenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = trim2Studenten.ResultKey,
                Operation = "select",
                OperationId = OpIdPdSingleSelectRows,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "select_value", "Insgesamt" },
                    { "column_name", "Typ" }
                },
            };
            trim2Studenten.Successors.Add(selectRowsStudenten);

            var join = new DoubleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetOneHash = selectRowsBerufsbildung.ResultKey,
                InputDatasetTwoHash = selectRowsStudenten.ResultKey,
                Operation = "join",
                OperationId = OpIdPdDoubleJoin,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "on", "Jahr" },
                    { "lsuffix", "_berufsbildung" },
                    { "rsuffix", "_studenten" }
                }
            };

            selectRowsStudenten.Successors.Add(join);
            selectRowsBerufsbildung.Successors.Add(join);

            var describe = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = join.ResultKey,
                Operation = "describe",
                OperationId = OpIdPdSingleGeneric,
            };
            join.Successors.Add(describe);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Chemnitz Students and Jobs",
                Root = new List<Node>
                {
                    trim1Berufsbildung,
                    trim1Studenten
                }
            };
        }

        public static Pipeline SimulatedVineYieldPipeline(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            var trimYield = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdSimulatedVineYield,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "first_n", "1" }
                },
            };

            var setHeader = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "set_header",
                OperationId = OpIdPdSingleMakeColumnHeader,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "header_row", "0" }
                }
            };
            Successor(trimYield, setHeader);

            var renameLabels = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "rename",
                OperationId = OpIdPdSingleRename,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "mapper", "{'nan':'Jahr'}" },
                    { "axis", "columns" }
                }
            };
            Successor(setHeader, renameLabels);

            var setIndex = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "set_index",
                OperationId = OpIdPdSingleSetIndex,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "keys", "Jahr" }
                }
            };
            Successor(renameLabels, setIndex);

            var mean = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "mean",
                OperationId = OpIdPdSingleMean,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "axis", "1" }
                }
            };

            Successor(setIndex, mean);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Simulated Vine Yield Styria",
                Root = new List<Node>
                {
                    trimYield
                }
            };
        }

        public static Pipeline ZamgWeatherPreprocessingGraz(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            var pipeline = new Pipeline
            {
                Name = "ZAMG Weather Data Preprocessing Graz 1990-2020",
                Id = pipelineId
            };

            for (var year = 1990; year <= 2020; year++)
            {
                pipeline.Root.Add(HardcodedNodes.ZamgWeatherPreprocessing(pipelineId, year));
            }

            return pipeline;
        }

        /// <summary>
        /// Makes <code>successor</code> the successor of <code>node</code>. 
        /// </summary>
        private static void Successor(NodeSingleInput node, NodeSingleInput successor)
        {
            node.Successors.Add(successor);
            successor.InputDatasetHash = node.ResultKey;
        }

        /// <summary>
        /// Makes <code>successor</code> the successor of <code>node</code>. 
        /// </summary>
        private static void Successor(NodeFileInput node, NodeSingleInput successor)
        {
            node.Successors.Add(successor);
            successor.InputDatasetHash = node.ResultKey;
        }
    }
}