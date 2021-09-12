using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;
using PipelineService.Services;
using PipelineService.Services.Impl;
using PipelineService.UnitTests.UnitTestHelpers;

namespace PipelineService.UnitTests.Services
{
    [TestFixture]
    public class PipelineDtoServiceUnitTests
    {
        private static NodeTupleTestCase[] _getNodeTupleTestCases =
        {
            new()
            {
                Pipeline = HardcodedDefaultPipelines.MelbourneHousingPipeline(),
                ExpectedSingleInputTuples = 3,
                ExpectedDoubleInputTuples = 0
            },
            new()
            {
                Pipeline = HardcodedDefaultPipelines.InfluenzaInterpolation(),
                ExpectedSingleInputTuples = 3,
                ExpectedDoubleInputTuples = 0
            },
            new()
            {
                Pipeline = HardcodedDefaultPipelines.ChemnitzStudentAndJobsPipeline(),
                ExpectedSingleInputTuples = 13,
                ExpectedDoubleInputTuples = 1
            }
        };

        private IPipelineDao _pipelineDao;
        private IPipelineExecutionDao _pipelineExecutionDao;
        private IPipelineDtoService _pipelineDtoService;

        [SetUp]
        public void SetUp()
        {
            var mock = new Mock<IPipelineExecutionDao>();
            mock.Setup(dao => dao.GetLastExecutionForPipeline(It.IsAny<Guid>()))
                .ReturnsAsync((Guid pipelineId) => new PipelineExecutionRecord
                {
                    PipelineId = pipelineId,
                    CompletedOn = DateTime.UtcNow
                });
            _pipelineExecutionDao = mock.Object;

            _pipelineDao = new InMemoryPipelineDao(GeneralHelper.CreateLogger<InMemoryPipelineDao>());
            _pipelineDtoService = new PipelineDtoService(_pipelineDao, _pipelineExecutionDao);
        }

        [Test]
        [TestCaseSource(nameof(_getNodeTupleTestCases))]
        public async Task GetSingleInputNodeTuples_ExecutedPipeline(NodeTupleTestCase testCase)
        {
            // arrange
            await _pipelineDao.Add(testCase.Pipeline);

            // act
            var results = await _pipelineDtoService.GetSingleInputNodeTuples(testCase.Pipeline.Id);

            // assert
            Assert.NotNull(results);
            Assert.AreEqual(testCase.ExpectedSingleInputTuples, results.Count);
        }
    }

    public class NodeTupleTestCase
    {
        public Pipeline Pipeline { get; set; }
        public int ExpectedSingleInputTuples { get; set; }
        public int ExpectedDoubleInputTuples { get; set; }
    }
}