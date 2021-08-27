using System;
using System.Collections.Generic;
using PipelineService.Extensions;
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

            var import = new NodeFileInput
            {
                InputObjectKey = "influenca_vienna_2009-2018.csv",
                InputObjectBucket = "defaultfiles",
                PipelineId = pipelineId,
                Operation = "read_csv",
                OperationId = OpIdPdFileInputReadCsv,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "header", "0" }
                }
            };

            var interpolate = new NodeSingleInput
            {
                PipelineId = pipelineId,
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

            var describe = new NodeSingleInput
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ResultKey,
                Operation = "describe",
                OperationId = OpIdPdSingleGeneric,
            };

            Successor(import, interpolate);
            Successor(interpolate, select);
            Successor(select, describe);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Influenza Interpolation",
                Root = new List<Node>
                {
                    import
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

            var import1Berufsbildung = new NodeFileInput
            {
                InputObjectKey = "21211-003Z_format.csv",
                InputObjectBucket = "defaultfiles",
                PipelineId = pipelineId,
                Operation = "read_csv",
                OperationId = OpIdPdFileInputReadCsv
            };

            // Data cleaning Berufsbildung
            var trim1Berufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "first_n", "6" }
                },
            };

            Successor(import1Berufsbildung, trim1Berufsbildung);

            var resetIndexBerufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "reset_index",
                OperationId = OpIdPdSingleResetIndex
            };

            Successor(trim1Berufsbildung, resetIndexBerufsbildung);

            var setHeaderBerufsbildung = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "set_header",
                OperationId = OpIdPdSingleMakeColumnHeader,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "header_row", "0" }
                }
            };

            Successor(resetIndexBerufsbildung, setHeaderBerufsbildung);

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

            var import1Studenten = new NodeFileInput
            {
                InputObjectKey = "21311-001Z_format.csv",
                InputObjectBucket = "defaultfiles",
                PipelineId = pipelineId,
                Operation = "read_csv",
                OperationId = OpIdPdFileInputReadCsv
            };
            // Data Cleaning Studenten
            var trim1Studenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "first_n", "6" }
                },
            };

            Successor(import1Studenten, trim1Studenten);

            var resetIndexStudenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "reset_index",
                OperationId = OpIdPdSingleResetIndex
            };
            Successor(trim1Studenten, resetIndexStudenten);

            var setHeaderStudenten = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "set_header",
                OperationId = OpIdPdSingleMakeColumnHeader,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "header_row", "0" }
                }
            };

            Successor(resetIndexStudenten, setHeaderStudenten);

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

            var join = new NodeDoubleInput
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

            var import = new NodeFileInput
            {
                InputObjectKey = "simulated-vine-yield-styria.xlsx",
                InputObjectBucket = "defaultfiles",
                PipelineId = pipelineId,
                Operation = "read_excel",
                OperationId = OpIdPdFileInputReadExcel,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "skiprows", "1" }
                }
            };

            var renameLabels = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "rename",
                OperationId = OpIdPdSingleRename,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "mapper", "{'Unnamed: 0':'Jahr'}" },
                    { "axis", "columns" }
                }
            };

            Successor(import, renameLabels);

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
                    import
                }
            };
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

            var nodesToJoin = new Queue<Node>();
            for (var year = 1990; year <= to; year++)
            {
                var import = HardcodedNodes.ZamgWeatherImport(pipelineId, year);
                var transpose = new NodeSingleInput
                {
                    PipelineId = pipelineId,
                    Operation = $"transpose_{year}",
                    OperationId = OpIdPdSingleTranspose
                };
                var trim = HardcodedNodes.ZamgWeatherTrim(pipelineId, year);
                var setHeader = new NodeSingleInput
                {
                    PipelineId = pipelineId,
                    Operation = $"set_header_{year}",
                    OperationId = OpIdPdSingleMakeColumnHeader,
                    OperationConfiguration = new Dictionary<string, string>
                    {
                        { "header_row", "Parameter" }
                    }
                };
                var setDateIndex = new NodeSingleInput
                {
                    PipelineId = pipelineId,
                    Operation = $"set_date_index_{year}",
                    OperationId = OpIdPdSingleSetDateIndex
                };
                Successor(import, transpose);
                Successor(transpose, trim);
                Successor(trim, setHeader);
                Successor(setHeader, setDateIndex);
                pipeline.Root.Add(import);
                nodesToJoin.Enqueue(setDateIndex);
            }

            // join nodes
            while (nodesToJoin.Count >= 2)
            {
                var nodeOne = nodesToJoin.Dequeue();
                var nodeTwo = nodesToJoin.Dequeue();
                var concat = new NodeDoubleInput
                {
                    PipelineId = pipelineId,
                    InputDatasetOneHash = nodeOne.ResultKey,
                    InputDatasetTwoHash = nodeTwo.ResultKey,
                    Operation = $"concat_{nodeOne.Operation.LastChars(4)}_{nodeTwo.Operation.LastChars(4)}",
                    OperationId = OpIdPdDoubleConcat
                };
                Successor(nodeOne, nodeTwo, concat);

                nodesToJoin.Enqueue(concat);
            }

            var sortIndex = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "sort_index",
                OperationId = OpIdPdSingleSortIndex
            };

            Successor(nodesToJoin.Dequeue(), sortIndex);

            var replaceStrings = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "replace",
                OperationId = OpIdPdSingleReplace,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "to_replace", "['Spuren']" },
                    { "value", "0" }
                }
            };

            Successor(sortIndex, replaceStrings);

            var toNumeric = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "to_numeric",
                OperationId = OpIdPdSingleDfToNumeric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "errors", "coerce" }
                }
            };

            Successor(replaceStrings, toNumeric);

            var interpolate = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "interpolate",
                OperationId = OpIdPdSingleInterpolate,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "axis", "0" },
                    { "method", "time" },
                }
            };

            Successor(toNumeric, interpolate);

            var resample = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "resample",
                OperationId = OpIdPdSingleResample,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "rule", "1Y" },
                    { "group_by_operation", "mean" },
                }
            };

            Successor(interpolate, resample);

            /*
            // Simulated Vine Yield
            var importVine = new NodeFileInput
            {
                InputObjectKey = "simulated-vine-yield-styria.xlsx",
                InputObjectBucket = "defaultfiles",
                PipelineId = pipelineId,
                Operation = "read_excel",
                OperationId = OpIdPdFileInputReadExcel,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "skiprows", "1" }
                }
            };

            var renameLabels = new NodeSingleInput
            {
                PipelineId = pipelineId,
                Operation = "rename",
                OperationId = OpIdPdSingleRename,
                OperationConfiguration = new Dictionary<string, string>
                {
                    { "mapper", "{'Unnamed: 0':'Jahr'}" },
                    { "axis", "columns" }
                }
            };

            Successor(importVine, renameLabels);

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

            var predict = new NodeDoubleInput
            {
                PipelineId = pipelineId,
                Operation = "predict_yield",
                OperationId = OpIdSkLearnDoubleMlpRegr
            };

            Successor(resample, setIndex, predict);
            pipeline.Root.Add(importVine);*/
            return pipeline;
        }

        /// <summary>
        /// Makes <code>successor</code> the successor of <code>node</code>. 
        /// </summary>
        private static void Successor(Node node, NodeSingleInput successor)
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

        private static void Successor(Node node1, Node node2, NodeDoubleInput successor)
        {
            node1.Successors.Add(successor);
            node2.Successors.Add(successor);
            successor.InputDatasetOneHash = node1.ResultKey;
            successor.InputDatasetTwoHash = node2.ResultKey;
        }
    }
}