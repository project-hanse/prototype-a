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
        private static readonly Guid DsIdChemnitzBerufsbildung1993 = Guid.Parse("2b88720f-8d2d-46c8-84d2-ab177c88cb5f");
        private static readonly Guid DsIdChemnitzStudenten1993 = Guid.Parse("61501213-d945-49a5-9212-506d6305af13");

        private static readonly Guid OpIdPdSingleGeneric = Guid.Parse("0759dede-2cee-433c-b314-10a8fa456e62");
        private static readonly Guid OpIdPdSingleTrim = Guid.Parse("5c9b34fc-ac4f-4290-9dfe-418647509559");
        private static readonly Guid OpIdPdSingleMakeColumnHeader = Guid.Parse("db8b6a9d-d01f-4328-b971-fa56ac350320");
        private static readonly Guid OpIdPdDoubleJoin = Guid.Parse("9acea312-713e-4de8-b8db-5d33613ab2f1");

        public static Pipeline MelbourneHousingPipeline(Guid pipelineId = default)
        {
            if (pipelineId == default)
            {
                pipelineId = Guid.NewGuid();
            }

            var cleanUp = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdMelbourneHousingFull,
                Operation = "dropna",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                },
            };

            var select = new SingleInputNode
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

            var describe = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ResultKey,
                Operation = "describe",
                OperationId = OpIdPdSingleGeneric,
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

            var interpolate = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdInfluencaVienna20092018,
                Operation = "interpolate",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"method", "linear"}
                },
            };

            var select = new SingleInputNode
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

            var unknownOperation = new SingleInputNode
            {
                PipelineId = pipeline.Id,
                InputDatasetId = DsIdMelbourneHousePricesLess,
                Operation = "unknown_operation",
                OperationId = Guid.NewGuid(), // random guid that does not represent an operation
                OperationConfiguration = new Dictionary<string, string>()
            };

            var describe = new SingleInputNode
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
            var trim1Berufsbildung = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdChemnitzBerufsbildung1993,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"first_n", "5"}
                },
            };

            var setHeaderBerufsbildung = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = trim1Berufsbildung.ResultKey,
                Operation = "set_header",
                OperationId = OpIdPdSingleMakeColumnHeader,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"header_row", "0"}
                }
            };
            trim1Berufsbildung.Successors.Add(setHeaderBerufsbildung);

            var dropNaBerufsbildung = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = setHeaderBerufsbildung.ResultKey,
                Operation = "dropna",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                },
            };
            setHeaderBerufsbildung.Successors.Add(dropNaBerufsbildung);

            var trim2Berufsbildung = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = dropNaBerufsbildung.ResultKey,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"first_n", "1"}
                },
            };

            dropNaBerufsbildung.Successors.Add(trim2Berufsbildung);

            // Data Cleaning Studenten
            var trim1Studenten = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetId = DsIdChemnitzStudenten1993,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"first_n", "5"}
                },
            };

            var setHeaderStudenten = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = trim1Berufsbildung.ResultKey,
                Operation = "set_header",
                OperationId = OpIdPdSingleMakeColumnHeader,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"header_row", "0"}
                }
            };
            trim1Studenten.Successors.Add(setHeaderStudenten);

            var dropNaStudenten = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = trim1Studenten.ResultKey,
                Operation = "dropna",
                OperationId = OpIdPdSingleGeneric,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                },
            };
            setHeaderStudenten.Successors.Add(dropNaStudenten);

            var trim2Studenten = new SingleInputNode
            {
                PipelineId = pipelineId,
                InputDatasetHash = dropNaStudenten.ResultKey,
                Operation = "trim",
                OperationId = OpIdPdSingleTrim,
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"first_n", "1"}
                },
            };
            dropNaStudenten.Successors.Add(trim2Studenten);

            var join = new DoubleInputNode
            {
                InputDatasetOneHash = trim2Berufsbildung.ResultKey,
                InputDatasetTwoHash = trim2Studenten.ResultKey,
                Operation = "join",
                OperationId = OpIdPdDoubleJoin,
                OperationConfiguration = new Dictionary<string, string>()
            };

            trim2Berufsbildung.Successors.Add(join);
            trim2Studenten.Successors.Add(join);

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
    }
}