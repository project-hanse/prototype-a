using System.Threading.Tasks;
using NUnit.Framework;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Models.Pipeline;
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
        private IPipelineDtoService _pipelineDtoService;

        [SetUp]
        public void SetUp()
        {
            _pipelineDao = new InMemoryPipelineDao(GeneralHelper.CreateLogger<InMemoryPipelineDao>());
            _pipelineDtoService = new PipelineDtoService(_pipelineDao);
        }

        [Test]
        [TestCaseSource(nameof(_getNodeTupleTestCases))]
        public async Task GetSingleInputNodeTuples(NodeTupleTestCase testCase)
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